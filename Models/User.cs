using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{

     public enum ActivityLevel
    {
        Sedentary,
        Light,
        Moderate,
        High
    }
    public class User
    {
        [Key]
        public string? user_uid { get; set; }

        [Required]
        [MaxLength(50)]
        public string? nickname { get; set; }

        public string? profile_picture { get; set; }
        public string? profile_description { get; set; }
        public string? gender { get; set; }
        public int? height { get; set; }
        public double? current_weight { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime? last_login { get; set; }
        public bool is_active { get; set; }

        [Required]
        [EnumDataType(typeof(ActivityLevel))]
        public ActivityLevel activity_level { get; set; }

        [Required]
        [Range(1900, 2025)]
        public int birth_year { get; set; }

        public User()
        {
            WeightMeasurements = new HashSet<WeightMeasurement>();
            UserGoals = new HashSet<UserGoal>();
            MealEntries = new HashSet<MealEntry>();
            ExerciseEntries = new HashSet<ExerciseEntry>();
            WaterIntakes = new HashSet<WaterIntake>();
            StreakHistories = new HashSet<StreakHistory>();
            UserConsultants = new HashSet<UserConsultant>();
            ConsultantNotes = new HashSet<ConsultantNote>();
            ConsultantRequests = new HashSet<ConsultantRequest>();
        }

        public virtual ICollection<WeightMeasurement> WeightMeasurements { get; set; }
        public virtual ICollection<UserGoal> UserGoals { get; set; }
        public virtual ICollection<MealEntry> MealEntries { get; set; }
        public virtual ICollection<ExerciseEntry> ExerciseEntries { get; set; }
        public virtual ICollection<WaterIntake> WaterIntakes { get; set; }
        public virtual ICollection<StreakHistory> StreakHistories { get; set; }
        public virtual ICollection<UserConsultant> UserConsultants { get; set; }
        public virtual ICollection<ConsultantNote> ConsultantNotes { get; set; }
        public virtual ICollection<ConsultantRequest> ConsultantRequests { get; set; }
    }

}