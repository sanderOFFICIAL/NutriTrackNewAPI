using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class MealEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int entry_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        [MaxLength(10)]
        public required string meal_type { get; set; } // breakfast, lunch, dinner, snack

        [Required]
        public DateTime entry_date { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(100)]
        public required string product_name { get; set; }

        [Required]
        public double quantity_grams { get; set; }

        [Required]
        public double calories { get; set; }

        [Required]
        public double protein { get; set; }

        [Required]
        public double carbs { get; set; }

        [Required]
        public double fats { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}