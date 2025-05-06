using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;
using NutriTrackAPI.Models;
using System;
using System.Threading.Tasks;

namespace NutriTrackAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GoalController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GoalController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-user-goal")]
        public async Task<IActionResult> CreateGoal([FromBody] CreateGoalRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.idToken);
                string uid = decodedToken.Uid;

                var user = await _context.Users.FindAsync(uid);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                if (!user.current_weight.HasValue || !user.height.HasValue || string.IsNullOrEmpty(user.gender) ||
                    user.birth_year == 0)
                {
                    return BadRequest(new { message = "User data is incomplete for goal creation (weight, height, gender, or birth year missing)." });
                }

                // Validate consultant_uid if provided
                if (!string.IsNullOrEmpty(request.consultant_uid))
                {
                    var consultant = await _context.Consultants.FindAsync(request.consultant_uid);
                    if (consultant == null)
                    {
                        return BadRequest(new { message = "Consultant not found." });
                    }
                }

                // Validate duration_weeks
                if (request.duration_weeks <= 0)
                {
                    return BadRequest(new { message = "Duration weeks must be greater than zero." });
                }

                var (calories, protein, carbs, fats, warning) = CalculateNutrition(
                    currentWeight: user.current_weight.Value,
                    targetWeight: request.target_weight,
                    durationWeeks: request.duration_weeks,
                    height: user.height.Value,
                    gender: user.gender,
                    goalType: request.goal_type,
                    activityLevel: user.activity_level,
                    birthYear: user.birth_year
                );

                var goal = new UserGoal
                {
                    user_uid = uid,
                    consultant_uid = request.consultant_uid,
                    goal_type = request.goal_type,
                    target_weight = request.target_weight,
                    duration_weeks = request.duration_weeks,
                    daily_calories = calories,
                    daily_protein = protein,
                    daily_carbs = carbs,
                    daily_fats = fats,
                    start_date = DateTime.UtcNow,
                    is_approved_by_consultant = request.consultant_uid == null,
                    User = user
                };

                _context.UserGoals.Add(goal);
                await _context.SaveChangesAsync();

                var response = MapToGoalResponse(goal);
                if (!string.IsNullOrEmpty(warning))
                {
                    response.warning = warning;
                }
                return CreatedAtAction(
                    actionName: nameof(GetGoal),
                    controllerName: "Goal",
                    routeValues: new { goalId = goal.goal_id },
                    value: response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-specific-goal-by-id/{goalId}")]
        public async Task<IActionResult> GetGoal(int goalId)
        {
            var goal = await _context.UserGoals
                .Include(g => g.User)
                .Include(g => g.Consultant)
                .FirstOrDefaultAsync(g => g.goal_id == goalId);

            if (goal == null)
            {
                return NotFound(new { message = "Goal not found." });
            }

            var response = MapToGoalResponse(goal);
            return Ok(response);
        }

        [HttpGet("get-all-user-goal-ids")]
        public async Task<IActionResult> GetUserGoalIds([FromQuery] string idToken)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string uid = decodedToken.Uid;

                var goalIds = await _context.UserGoals
                    .Where(g => g.user_uid == uid)
                    .OrderByDescending(g => g.start_date)
                    .Select(g => new { goal_id = g.goal_id })
                    .ToListAsync();

                if (!goalIds.Any())
                {
                    return NotFound(new { message = "No goals found for this user." });
                }

                return Ok(goalIds);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("get-goal-id-by-user-uid/{userUid}")]
        public async Task<IActionResult> GetGoalIdByUserUid(string userUid)
        {
            try
            {
                var goal = await _context.UserGoals
                    .FirstOrDefaultAsync(g => g.user_uid == userUid);

                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found for the specified user." });
                }

                return Ok(new { goal_id = goal.goal_id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("update-goal-weight")]
        public async Task<IActionResult> UpdateUserWeight([FromBody] UpdateWeightRequest request, [FromQuery] string idToken)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string userId = decodedToken.Uid;

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                if (!user.current_weight.HasValue || !user.height.HasValue || string.IsNullOrEmpty(user.gender) ||
                    user.birth_year == 0)
                {
                    return BadRequest(new { message = "User data is incomplete for goal recalculation (weight, height, gender, or birth year missing)." });
                }

                var userGoal = await _context.UserGoals
                    .FirstOrDefaultAsync(g => g.goal_id == request.goal_id && g.user_uid == userId);

                if (userGoal == null)
                {
                    return NotFound(new { message = "Goal not found for the specified goal ID and user." });
                }

                // Validate duration_weeks
                if (userGoal.duration_weeks <= 0)
                {
                    return BadRequest(new { message = "Duration weeks must be greater than zero." });
                }

                userGoal.target_weight = request.new_weight;
                _context.Entry(userGoal).State = EntityState.Modified;

                var (calories, protein, carbs, fats, warning) = CalculateNutrition(
                    currentWeight: user.current_weight.Value,
                    targetWeight: userGoal.target_weight,
                    durationWeeks: userGoal.duration_weeks,
                    height: user.height.Value,
                    gender: user.gender,
                    goalType: userGoal.goal_type,
                    activityLevel: user.activity_level,
                    birthYear: user.birth_year
                );
                userGoal.daily_calories = calories;
                userGoal.daily_protein = protein;
                userGoal.daily_carbs = carbs;
                userGoal.daily_fats = fats;

                _context.Entry(userGoal).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                var response = new { message = "Target weight updated and goal recalculated successfully.", warning = warning };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("approve-goal-by-consultant")]
        public async Task<IActionResult> ApproveGoal([FromBody] ApproveGoalRequest request)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                string consultantUid = decodedToken.Uid;

                var goal = await _context.UserGoals
                    .FirstOrDefaultAsync(g => g.goal_id == request.GoalId && g.consultant_uid == consultantUid);

                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found or consultant not authorized to approve this goal." });
                }

                goal.is_approved_by_consultant = true;

                _context.Entry(goal).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Goal successfully approved." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("delete-goal/{goalId}")]
        public async Task<IActionResult> DeleteGoal(int goalId, [FromQuery] string idToken)
        {
            try
            {
                FirebaseService.Initialize();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
                string userId = decodedToken.Uid;

                var goal = await _context.UserGoals
                    .FirstOrDefaultAsync(g => g.goal_id == goalId && g.user_uid == userId);

                if (goal == null)
                {
                    return NotFound(new { message = "Goal not found or user not authorized to delete this goal." });
                }

                _context.UserGoals.Remove(goal);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Goal successfully deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        private GoalResponse MapToGoalResponse(UserGoal goal)
        {
            return new GoalResponse
            {
                goal_id = goal.goal_id,
                user_uid = goal.user_uid,
                consultant_uid = goal.consultant_uid,
                goal_type = goal.goal_type,
                target_weight = goal.target_weight,
                duration_weeks = goal.duration_weeks,
                daily_calories = goal.daily_calories,
                daily_protein = goal.daily_protein,
                daily_carbs = goal.daily_carbs,
                daily_fats = goal.daily_fats,
                start_date = goal.start_date,
                is_approved_by_consultant = goal.is_approved_by_consultant,
                user = goal.User != null ? new UserBasicInfo
                {
                    user_uid = goal.User.user_uid,
                    nickname = goal.User.nickname,
                    profile_picture = goal.User.profile_picture
                } : null,
                consultant = goal.Consultant != null ? new ConsultantBasicInfo
                {
                    user_uid = goal.Consultant.consultant_uid,
                    nickname = goal.Consultant.nickname,
                    profile_picture = goal.Consultant.profile_picture
                } : null
            };
        }

        private (double calories, double protein, double carbs, double fats, string warning) CalculateNutrition(
            double currentWeight,
            double targetWeight,
            int durationWeeks,
            int height,
            string gender,
            GoalType goalType,
            ActivityLevel activityLevel,
            int birthYear)
        {
            const double CaloriesPerKg = 7700; // Кількість калорій на 1 кг ваги
            string warning = null;

            // Calculate age
            int currentYear = DateTime.UtcNow.Year;
            int age = currentYear - birthYear;

            // Calculate BMR using Mifflin-St Jeor formula
            double bmr;
            if (gender.ToLower() == "male")
            {
                bmr = (10 * currentWeight) + (6.25 * height) - (5 * age) + 5;
            }
            else
            {
                bmr = (10 * currentWeight) + (6.25 * height) - (5 * age) - 161;
            }

            // Calculate TDEE based on activity level
            double tdee = bmr;
            switch (activityLevel)
            {
                case ActivityLevel.Sedentary:
                    tdee *= 1.2;
                    break;
                case ActivityLevel.Light:
                    tdee *= 1.37;
                    break;
                case ActivityLevel.Moderate:
                    tdee *= 1.42;
                    break;
                case ActivityLevel.High:
                    tdee *= 1.62;
                    break;
            }

            // Calculate weight difference and required calorie adjustment
            double weightDifference = targetWeight - currentWeight; // Позитивне для набору, негативне для втрати
            double totalCaloriesNeeded = Math.Abs(weightDifference) * CaloriesPerKg; // Загальні калорії для зміни ваги
            double weeklyCalorieAdjustment = totalCaloriesNeeded / durationWeeks; // Тижневий дефіцит/надлишок
            double dailyCalorieAdjustment = weeklyCalorieAdjustment / 7; // Денний дефіцит/надлишок

            // Warn if calorie adjustment is unrealistic
            if (dailyCalorieAdjustment > 2000)
            {
                warning = $"Warning: Daily calorie adjustment ({dailyCalorieAdjustment:F2} kcal) is highly unrealistic. Consider increasing duration to at least {Math.Ceiling(Math.Abs(weightDifference) / 0.5)} weeks for safer weight change.";
            }

            // Adjust daily calories based on goal type
            double dailyCalories = tdee;
            if (goalType == GoalType.Loss)
            {
                dailyCalories -= dailyCalorieAdjustment;
                dailyCalories = Math.Max(dailyCalories, gender.ToLower() == "male" ? 1500 : 1200);
            }
            else if (goalType == GoalType.Gain)
            {
                dailyCalories += dailyCalorieAdjustment;
            }
            // For Maintain, dailyCalories remains TDEE

            // Calculate macronutrients based on goal type
            double proteinPerKg;
            double fatPercentage;
            double carbPercentage;
            switch (goalType)
            {
                case GoalType.Loss:
                    proteinPerKg = 1.8; // 1.8 г/кг для втрати ваги
                    fatPercentage = 0.20; // 20% калорій від жирів
                    carbPercentage = 0.40; // 40% калорій від вуглеводів
                    break;
                case GoalType.Gain:
                    proteinPerKg = 1.9; // 2.2 г/кг для набору ваги
                    fatPercentage = 0.25; // 25% калорій від жирів
                    carbPercentage = 0.45; // 45% калорій від вуглеводів
                    break;
                case GoalType.Maintain:
                default:
                    proteinPerKg = 1.4; // 1.4 г/кг для підтримки
                    fatPercentage = 0.30; // 30% калорій від жирів
                    carbPercentage = 0.40; // 40% калорій від вуглеводів
                    break;
            }

            // Adjust proteinPerKg based on weight difference and duration
            double averageWeight = (currentWeight + targetWeight) / 2;
            if (Math.Abs(weightDifference) > 20)
            {
                proteinPerKg += 0.3; // Бонус для великих змін ваги
            }
            if (durationWeeks < 10)
            {
                proteinPerKg += 0.1; // Бонус для коротких періодів
            }

            // Calculate protein based on average weight
            double protein = averageWeight * proteinPerKg;

            // Warn if protein intake is extremely high
            if (protein / averageWeight > 3)
            {
                warning = (string.IsNullOrEmpty(warning) ? "" : warning + " ") +
                         $"Warning: Protein intake ({protein:F2} g, {protein / averageWeight:F2} g/kg) is extremely high and may not be sustainable.";
            }

            // Calculate fats and carbs
            double fats = (dailyCalories * fatPercentage) / 9;
            double proteinCalories = protein * 4;
            double fatCalories = fats * 9;
            double carbCalories = dailyCalories * carbPercentage;
            double carbs = carbCalories / 4;

            // Ensure non-negative macronutrients
            protein = Math.Max(protein, 0);
            fats = Math.Max(fats, 0);
            carbs = Math.Max(carbs, 0);

            return (dailyCalories, protein, carbs, fats, warning);
        }

        // DTOs
        public class CreateGoalRequest
        {
            public required string idToken { get; set; }
            public string? consultant_uid { get; set; }
            public GoalType goal_type { get; set; }
            public double target_weight { get; set; }
            public int duration_weeks { get; set; }
        }

        public class GoalResponse
        {
            public int goal_id { get; set; }
            public required string user_uid { get; set; }
            public string? consultant_uid { get; set; }
            public GoalType goal_type { get; set; }
            public double target_weight { get; set; }
            public int duration_weeks { get; set; }
            public double daily_calories { get; set; }
            public double daily_protein { get; set; }
            public double daily_carbs { get; set; }
            public double daily_fats { get; set; }
            public DateTime start_date { get; set; }
            public bool is_approved_by_consultant { get; set; }
            public UserBasicInfo? user { get; set; }
            public ConsultantBasicInfo? consultant { get; set; }
            public string? warning { get; set; }
        }

        public class UserBasicInfo
        {
            public required string user_uid { get; set; }
            public required string nickname { get; set; }
            public string? profile_picture { get; set; }
        }

        public class ConsultantBasicInfo
        {
            public required string user_uid { get; set; }
            public required string nickname { get; set; }
            public string? profile_picture { get; set; }
        }

        public class UpdateWeightRequest
        {
            public int goal_id { get; set; }
            public double new_weight { get; set; }
        }

        public class ApproveGoalRequest
        {
            public required string IdToken { get; set; }
            public int GoalId { get; set; }
        }

        public class DeleteGoalRequest
        {
            public int GoalId { get; set; }
            public string IdToken { get; set; }
        }

    }
}