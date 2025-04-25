using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class ConsultantNote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int note_id { get; set; }

        [Required]
        public required string consultant_uid { get; set; }

        [Required]
        public int goal_id { get; set; }

        [Required]
        [MaxLength(1000)]
        public required string content { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public required string user_uid { get; set; }
        [ForeignKey("user_uid")]
        public virtual required User User { get; set; }
        [ForeignKey("consultant_uid")]
        public virtual required Consultant Consultant { get; set; }

        [ForeignKey("goal_id")]
        public virtual required UserGoal UserGoal { get; set; }
    }
}