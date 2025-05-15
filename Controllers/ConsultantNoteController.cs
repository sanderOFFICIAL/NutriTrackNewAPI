using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using FirebaseAdmin.Auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsultantNoteController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ConsultantNoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-note")]
        public async Task<IActionResult> AddNote([FromBody] CreateConsultantNoteRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                string consultantId = decodedToken.Uid;

                var goal = await _context.UserGoals
                    .Include(g => g.User)
                    .FirstOrDefaultAsync(g => g.goal_id == request.goal_id);

                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found." });
                }

                var consultantRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(cr =>
                        cr.consultant_uid == consultantId &&
                        cr.user_uid == goal.User.user_uid &&
                        cr.status == "accepted");

                if (consultantRequest == null)
                {
                    return BadRequest(new { message = "Consultation request must be accepted." });
                }

                var note = new ConsultantNote
                {
                    consultant_uid = consultantId,
                    goal_id = request.goal_id,
                    content = request.content,
                    created_at = DateTime.UtcNow,
                    user_uid = goal.User.user_uid,
                    User = goal.User,
                    UserGoal = goal,
                    Consultant = await _context.Consultants.FirstOrDefaultAsync(c => c.consultant_uid == consultantId)
                };
                if (note.Consultant == null)
                {
                    return NotFound(new { message = "Consultant not found." });
                }
                _context.ConsultantNotes.Add(note);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Note added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-note")]
        public async Task<IActionResult> UpdateNote([FromBody] UpdateConsultantNoteRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                string consultantId = decodedToken.Uid;

                var note = await _context.ConsultantNotes
                    .Include(n => n.UserGoal)
                    .ThenInclude(g => g.User)
                    .FirstOrDefaultAsync(n => n.note_id == request.note_id);

                if (note == null)
                {
                    return NotFound(new { message = "Note not found." });
                }

                var consultantRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(cr =>
                        cr.consultant_uid == consultantId &&
                        cr.user_uid == note.UserGoal.User.user_uid &&
                        cr.status == "accepted");

                if (consultantRequest == null)
                {
                    return BadRequest(new { message = "Consultation request must be accepted." });
                }

                if (note.consultant_uid != consultantId)
                {
                    return Unauthorized(new { message = "You are not authorized to update this note." });
                }

                note.content = request.content;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Note updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-notes")]
        public async Task<IActionResult> GetNotes([FromQuery] int goalId)
        {
            var notes = await _context.ConsultantNotes
                .Include(n => n.Consultant)
                .Where(n => n.goal_id == goalId)
                .OrderByDescending(n => n.created_at)
                .Select(n => new
                {
                    n.note_id,
                    consultant_nickname = n.Consultant.nickname,
                    n.consultant_uid,
                    n.goal_id,
                    n.content,
                    n.created_at
                })
                .ToListAsync();

            if (!notes.Any())
            {
                return NotFound(new { message = "No notes found for this goal." });
            }

            return Ok(notes);
        }


        [HttpDelete("delete-note")]
        public async Task<IActionResult> DeleteNote([FromQuery] string idToken, [FromQuery] int note_id)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string consultantId = decodedToken.Uid;

                var note = await _context.ConsultantNotes
                    .Include(n => n.UserGoal)
                    .ThenInclude(g => g.User)
                    .FirstOrDefaultAsync(n => n.note_id == note_id);

                if (note == null)
                {
                    return NotFound(new { message = "Note not found." });
                }

                var consultantRequest = await _context.ConsultantRequests
                    .FirstOrDefaultAsync(cr =>
                        cr.consultant_uid == consultantId &&
                        cr.user_uid == note.UserGoal.User.user_uid &&
                        cr.status == "accepted");

                if (consultantRequest == null)
                {
                    return BadRequest(new { message = "Consultation request must be accepted." });
                }

                if (note.consultant_uid != consultantId)
                {
                    return Unauthorized(new { message = "You are not authorized to delete this note." });
                }

                _context.ConsultantNotes.Remove(note);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Note deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        public class CreateConsultantNoteRequest
        {

            public required string IdToken { get; set; }


            public int goal_id { get; set; }


            public required string content { get; set; }
        }

        public class UpdateConsultantNoteRequest
        {

            public required string IdToken { get; set; }


            public int note_id { get; set; }

            public required string content { get; set; }
        }

        public class GetConsultantNotesRequest
        {
            public int goal_id { get; set; }
        }

        public class DeleteConsultantNoteRequest
        {
            public required string IdToken { get; set; }

            public int note_id { get; set; }
        }
    }
}
