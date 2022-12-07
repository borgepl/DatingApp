using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required] public string KnownAs { get; set; }
        [Required] public string Gender { get; set; }
        
        [JsonConverter(typeof(DateOnlyJsonConverter))]
        [Required] public DateOnly? DateOfBirth { get; set; } // optional to make required work for birth date
        [Required] public string City { get; set; }
        [Required] public string Country { get; set; }

        [Required]
        [StringLength(8, MinimumLength = 4)]
        public string Password { get; set; }
    }
}