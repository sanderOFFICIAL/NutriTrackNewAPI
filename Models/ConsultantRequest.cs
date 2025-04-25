using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class ConsultantRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int request_id { get; set; }

        [Required]
        public required string consultant_uid { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        public required string status { get; set; } // "pending", "accepted", "rejected"

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime responded_at { get; set; } = DateTime.UtcNow;


        [ForeignKey("consultant_uid")]
        public virtual required Consultant Consultant { get; set; }

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
    }
}