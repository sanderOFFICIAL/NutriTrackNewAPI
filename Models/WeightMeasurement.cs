using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class WeightMeasurement
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int measurement_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        public double weight { get; set; }

        public DateTime measured_at { get; set; } = DateTime.UtcNow;
        public required string device_id { get; set; }
        public bool is_synced { get; set; }

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}