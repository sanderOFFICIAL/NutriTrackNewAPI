using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExerciseController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-exercise")]
        public async Task<IActionResult> AddExercise([FromBody] AddExerciseRequest request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var exerciseEntry = new ExerciseEntry
            {
                user_uid = userId,
                exercise_name = request.exercise_name,
                duration_minutes = request.duration_minutes,
                calories_burned = request.calories_burned,
                exercise_type = request.exercise_type,
                entry_date = request.entry_date,
                User = user
            };

            _context.ExerciseEntries.Add(exerciseEntry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exercise entry added successfully." });
        }

        [HttpPut("update-exercise")]
        public async Task<IActionResult> UpdateExercise([FromBody] UpdateExerciseRequest request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var exerciseEntry = await _context.ExerciseEntries.FindAsync(request.ExerciseId);
            if (exerciseEntry == null || exerciseEntry.user_uid != userId)
            {
                return NotFound(new { message = "Exercise entry not found." });
            }

            exerciseEntry.exercise_name = request.exercise_name;
            exerciseEntry.duration_minutes = request.duration_minutes;
            exerciseEntry.calories_burned = request.calories_burned;
            exerciseEntry.exercise_type = request.exercise_type;
            exerciseEntry.entry_date = request.entry_date;

            _context.Entry(exerciseEntry).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exercise entry updated successfully." });
        }
        [HttpGet("get-exercises")]
        public async Task<IActionResult> GetExercises([FromQuery] string IdToken)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(IdToken);
            string userId = decodedToken.Uid;

            var exerciseEntries = await _context.ExerciseEntries
                .Where(e => e.user_uid == userId)
                .ToListAsync();

            if (exerciseEntries == null || exerciseEntries.Count == 0)
            {
                return NotFound(new { message = "No exercises found for the user." });
            }

            return Ok(exerciseEntries);
        }


        [HttpDelete("delete-exercise")]
        public async Task<IActionResult> DeleteExercise([FromBody] DeleteExerciseRequest request)
        {

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var exerciseEntry = await _context.ExerciseEntries.FindAsync(request.ExerciseId);
            if (exerciseEntry == null || exerciseEntry.user_uid != userId)
            {
                return NotFound(new { message = "Exercise entry not found." });
            }

            _context.ExerciseEntries.Remove(exerciseEntry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Exercise entry deleted successfully." });
        }

    }

    public class AddExerciseRequest
    {
        public required string exercise_name { get; set; }
        public int duration_minutes { get; set; }
        public float calories_burned { get; set; }
        public required string exercise_type { get; set; }
        public DateTime entry_date { get; set; }
        public required string IdToken { get; set; }
    }


    public class UpdateExerciseRequest
    {
        public required string exercise_name { get; set; }
        public int duration_minutes { get; set; }
        public float calories_burned { get; set; }
        public required string exercise_type { get; set; }
        public DateTime entry_date { get; set; }
        public int ExerciseId { get; set; }
        public required string IdToken { get; set; }
    }

    public class DeleteExerciseRequest
    {
        public int ExerciseId { get; set; }
        public required string IdToken { get; set; }
    }

    public class GetExercisesRequest
    {
        public required string IdToken { get; set; }
    }
}
