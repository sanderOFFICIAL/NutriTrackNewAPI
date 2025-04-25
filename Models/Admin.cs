using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NutriTrackAPI.Models
{
    public class Admin
    {
        [Key]
        public required string admin_uid { get; set; }

        [Required]
        public DateTime registration_date { get; set; }

        [Required]
        [MaxLength(100)]
        public required string name { get; set; }
        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public required string email { get; set; }

        [Required]
        [RegularExpression(@"^\+380\d{9}$", ErrorMessage = "Phone number must be in the format +380XXXXXXXXX")]
        [MaxLength(20)]
        public required string phone_number { get; set; }
    }
}