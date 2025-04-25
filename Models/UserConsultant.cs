using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class UserConsultant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int user_consultant_id { get; set; }

        [Required]
        public required string user_uid { get; set; }

        [Required]
        public required string consultant_uid { get; set; }

        public DateTime assignment_date { get; set; } = DateTime.UtcNow;

        public bool is_active { get; set; }

        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }

        [ForeignKey("consultant_uid")]
        public virtual required Consultant Consultant { get; set; }
    }
}