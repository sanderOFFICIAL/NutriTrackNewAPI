using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeightMeasurementsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WeightMeasurementsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostWeightMeasurement([FromBody] WeightMeasurementRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid data.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.user_uid == request.UserUid);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var weightMeasurement = new WeightMeasurement
            {
                user_uid = request.UserUid,
                weight = request.Weight,
                measured_at = request.MeasuredAt,
                device_id = request.DeviceId,
                is_synced = request.IsSynced,
                User = user
            };

            _context.WeightMeasurements.Add(weightMeasurement);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWeightMeasurement), new { id = weightMeasurement.measurement_id }, weightMeasurement);
        }

        [HttpGet("user/{userUid}")]
        public async Task<ActionResult<IQueryable<WeightMeasurement>>> GetWeightMeasurementsByUserUid(string userUid)
        {
            var weightMeasurements = await _context.WeightMeasurements
                .Where(wm => wm.user_uid == userUid)
                .ToListAsync();

            if (weightMeasurements == null || !weightMeasurements.Any())
            {
                return NotFound("No weight measurements found for this user.");
            }

            return Ok(weightMeasurements);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WeightMeasurement>> GetWeightMeasurement(int id)
        {
            var weightMeasurement = await _context.WeightMeasurements.FindAsync(id);

            if (weightMeasurement == null)
            {
                return NotFound();
            }

            return weightMeasurement;
        }
    }

    public class WeightMeasurementRequest
    {
        public required string UserUid { get; set; }
        public double Weight { get; set; }
        public DateTime MeasuredAt { get; set; }
        public required string DeviceId { get; set; }
        public bool IsSynced { get; set; }
    }
}
