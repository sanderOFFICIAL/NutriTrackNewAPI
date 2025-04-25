using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NutriTrackAPI.Models
{
    public class ExerciseEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int exercise_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        [MaxLength(100)]
        public required string exercise_name { get; set; }

        [Required]
        public int duration_minutes { get; set; }

        [Required]
        public double calories_burned { get; set; }

        public required string exercise_type { get; set; }

        [Required]
        public DateTime entry_date { get; set; } = DateTime.UtcNow;

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}