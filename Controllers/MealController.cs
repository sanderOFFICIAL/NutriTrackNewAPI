using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using FirebaseAdmin.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NutriTrack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MealController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MealController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("add-meal")]
        public async Task<IActionResult> AddMeal([FromBody] CreateMealRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                string uid = decodedToken.Uid;

                var user = await _context.Users.FindAsync(uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                foreach (var product in request.products)
                {
                    var mealEntry = new MealEntry
                    {
                        user_uid = uid,
                        meal_type = request.meal_type,
                        entry_date = DateTime.UtcNow.Date,
                        product_name = product.product_name,
                        quantity_grams = product.quantity_grams,
                        calories = (product.calories / 100) * product.quantity_grams,
                        protein = (product.protein / 100) * product.quantity_grams,
                        carbs = (product.carbs / 100) * product.quantity_grams,
                        fats = (product.fats / 100) * product.quantity_grams,
                        created_at = DateTime.UtcNow,
                        User = user
                    };

                    _context.MealEntries.Add(mealEntry);
                }

                await _context.SaveChangesAsync();
                return Ok(new { message = "Meal and products added successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("delete-meal")]
        public async Task<IActionResult> DeleteMeal([FromBody] DeleteMealRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                string uid = decodedToken.Uid;

                var user = await _context.Users.FindAsync(uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                var query = _context.MealEntries.Where(me => me.user_uid == uid);

                if (request.EntryId.HasValue)
                {
                    query = query.Where(me => me.entry_id == request.EntryId.Value);
                }
                else
                {
                    return BadRequest(new { message = "Either entryId must be provided." });
                }

                var mealsToDelete = await query.ToListAsync();

                if (!mealsToDelete.Any())
                {
                    return NotFound(new { message = "No meal entries found matching the provided criteria." });
                }

                _context.MealEntries.RemoveRange(mealsToDelete);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Meal entries deleted successfully.", deletedCount = mealsToDelete.Count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-all-meals")]
        public async Task<IActionResult> GetAllMealsForUser([FromQuery] string idToken)
        {
            FirebaseService.Initialize();
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
            string userId = decodedToken.Uid;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var meals = await _context.MealEntries
                .Where(me => me.user_uid == userId)
                .OrderByDescending(me => me.entry_date)
                .Select(me => new
                {
                    me.entry_id,
                    me.meal_type,
                    me.entry_date,
                    me.product_name,
                    me.quantity_grams,
                    me.calories,
                    me.protein,
                    me.carbs,
                    me.fats,
                    me.created_at
                })
                .ToListAsync();

            if (!meals.Any())
            {
                return NotFound(new { message = "No meal entries found." });
            }

            return Ok(meals);
        }
    }

    public class CreateMealRequest
    {
        public required string IdToken { get; set; }
        public required string meal_type { get; set; }
        public required List<MealProductRequest> products { get; set; }
    }

    public class DeleteMealRequest
    {
        public required string IdToken { get; set; }
        public int? EntryId { get; set; }
    }

    public class MealProductRequest
    {
        public required string product_name { get; set; }
        public double quantity_grams { get; set; }
        public double calories { get; set; }
        public double protein { get; set; }
        public double carbs { get; set; }
        public double fats { get; set; }
    }
}
