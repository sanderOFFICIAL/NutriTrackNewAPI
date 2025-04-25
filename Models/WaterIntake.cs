using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    [Table("WaterIntake")]
    public class WaterIntake
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int intake_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        public double amount_ml { get; set; }

        [Required]
        public DateTime entry_date { get; set; } = DateTime.UtcNow;

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}