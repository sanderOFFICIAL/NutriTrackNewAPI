using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    [Table("StreakHistory")]
    public class StreakHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int streak_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        public DateTime streak_date { get; set; } = DateTime.UtcNow;

        [Required]
        public int current_streak { get; set; }

        public bool is_active { get; set; }

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}