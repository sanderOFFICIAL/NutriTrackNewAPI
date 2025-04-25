using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using FirebaseAdmin.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StreakController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StreakController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-streak")]
        public async Task<IActionResult> AddStreak([FromBody] AddStreakRequest request)
        {

            FirebaseService.Initialize();

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var streakHistory = new StreakHistory
            {
                user_uid = userId,
                streak_date = DateTime.UtcNow,
                current_streak = 1,
                is_active = true,
                User = user
            };

            _context.StreakHistories.Add(streakHistory);
            await _context.SaveChangesAsync();

            return Ok(new { message = "New streak started successfully." });
        }

        [HttpPut("update-streak")]
        public async Task<IActionResult> UpdateStreak([FromBody] UpdateStreakRequest request)
        {
            FirebaseService.Initialize();

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var streakHistory = await _context.StreakHistories
                .Where(s => s.user_uid == userId)
                .OrderByDescending(s => s.streak_date)
                .FirstOrDefaultAsync();

            if (streakHistory == null)
            {
                return NotFound(new { message = "No active streak found." });
            }

            streakHistory.current_streak = request.current_streak;
            streakHistory.streak_date = DateTime.UtcNow;
            streakHistory.is_active = request.is_active;

            _context.Entry(streakHistory).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Streak updated successfully." });
        }

        [HttpGet("get-streaks")]
        public async Task<IActionResult> GetStreaks([FromQuery] string idToken)
        {
            FirebaseService.Initialize();

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var streakHistories = await _context.StreakHistories
                .Where(s => s.user_uid == userId)
                .OrderByDescending(s => s.streak_date)
                .Select(s => new
                {
                    s.streak_id,
                    s.streak_date,
                    s.current_streak,
                    s.is_active
                })
                .ToListAsync();

            return Ok(streakHistories);
        }


        [HttpDelete("disable-streak")]
        public async Task<IActionResult> DeleteStreak([FromBody] IdTokenRequest request)
        {
            FirebaseService.Initialize();

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var streakHistory = await _context.StreakHistories
                .Where(s => s.user_uid == userId && s.is_active)
                .OrderByDescending(s => s.streak_date)
                .FirstOrDefaultAsync();

            if (streakHistory == null)
            {
                return NotFound(new { message = "No active streak found." });
            }

            streakHistory.is_active = false;
            _context.Entry(streakHistory).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Streak deleted successfully." });
        }
    }


    public class AddStreakRequest
    {
        public required string IdToken { get; set; }
        public int current_streak { get; set; }
    }

    public class UpdateStreakRequest
    {
        public required string IdToken { get; set; }
        public int current_streak { get; set; }
        public bool is_active { get; set; }
    }

    public class IdTokenRequest
    {
        public required string IdToken { get; set; }
    }
}
