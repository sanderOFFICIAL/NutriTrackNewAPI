using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NutriTrackAPI.Models
{
    public class Consultant
    {
        [Key]
        public string? consultant_uid { get; set; }

        [Required]

        [MaxLength(50)]
        public string? nickname { get; set; }

        public string? profile_picture { get; set; }

        public string? profile_description { get; set; }

        public string? gender { get; set; }

        [Required]
        public int experience_years { get; set; }

        public bool is_active { get; set; }
        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime? last_login { get; set; }

        [Required]
        public int max_clients { get; set; }

        public int current_clients { get; set; }

        public Consultant()
        {
            UserGoals = new HashSet<UserGoal>();
            UserConsultants = new HashSet<UserConsultant>();
            ConsultantNotes = new HashSet<ConsultantNote>();
            ConsultantRequests = new HashSet<ConsultantRequest>();
        }

        public virtual ICollection<UserGoal> UserGoals { get; set; }
        public virtual ICollection<UserConsultant> UserConsultants { get; set; }
        public virtual ICollection<ConsultantNote> ConsultantNotes { get; set; }
        public virtual ICollection<ConsultantRequest> ConsultantRequests { get; set; }
    }
}