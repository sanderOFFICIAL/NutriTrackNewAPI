using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Auth;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WaterController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WaterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-water")]
        public async Task<IActionResult> AddWater([FromBody] AddWaterRequest request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            var waterIntake = new WaterIntake
            {
                user_uid = userId,
                amount_ml = request.amount_ml,
                entry_date = request.entry_date,
                User = user
            };

            _context.WaterIntakes.Add(waterIntake);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Water intake added successfully." });
        }

        [HttpPut("update-water")]
        public async Task<IActionResult> UpdateWater([FromBody] UpdateWaterRequest request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var waterIntake = await _context.WaterIntakes.FindAsync(request.intakeId);
            if (waterIntake == null || waterIntake.user_uid != userId)
            {
                return NotFound(new { message = "Water intake entry not found." });
            }

            waterIntake.amount_ml = request.amount_ml;

            _context.Entry(waterIntake).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Water intake updated successfully." });
        }
        [HttpDelete("delete-water")]
        public async Task<IActionResult> DeleteWater([FromBody] DeleteWaterRequest request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var waterIntake = await _context.WaterIntakes.FindAsync(request.intakeId);
            if (waterIntake == null || waterIntake.user_uid != userId)
            {
                return NotFound(new { message = "Water intake entry not found." });
            }

            _context.WaterIntakes.Remove(waterIntake);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Water intake entry deleted successfully." });
        }

        [HttpGet("get-water")]
        public async Task<IActionResult> GetWater([FromQuery] string idToken)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            var waterIntakes = await _context.WaterIntakes
                .Where(w => w.user_uid == userId)
                .Select(w => new
                {
                    w.intake_id,
                    w.amount_ml,
                    w.entry_date
                })
                .ToListAsync();

            return Ok(waterIntakes);
        }
    }

    public class AddWaterRequest
    {
        public required string IdToken { get; set; }
        public double amount_ml { get; set; }
        public DateTime entry_date { get; set; }
    }

    public class UpdateWaterRequest
    {
        public required string IdToken { get; set; }
        public int intakeId { get; set; }
        public double amount_ml { get; set; }
    }

    public class DeleteWaterRequest
    {
        public required string IdToken { get; set; }
        public int intakeId { get; set; }
    }
}
