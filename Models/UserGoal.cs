using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public enum GoalType
    {
        Gain,
        Loss,
        Maintain
    }

    public class UserGoal
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int goal_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        public string? consultant_uid { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(10)")]
        public GoalType goal_type { get; set; }

        [Required]
        public double target_weight { get; set; }

        [Required]
        public int duration_weeks { get; set; }

        [Required]
        public double daily_calories { get; set; }

        [Required]
        public double daily_protein { get; set; }

        [Required]
        public double daily_carbs { get; set; }

        [Required]
        public double daily_fats { get; set; }

        [Required]
        [Column(TypeName = "date")]
        public DateTime start_date { get; set; }

        [Required]
        public bool is_approved_by_consultant { get; set; }

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }

        [ForeignKey("consultant_uid")]
        public virtual Consultant? Consultant { get; set; }

        public virtual ICollection<ConsultantNote>? ConsultantNotes { get; set; }
    }
}