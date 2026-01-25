using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.DTOs
{

    public class EditProfileDto
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;

        
        //public string? PhoneNumber { get; set; }
        //public string? AvatarUrl { get; set; }

        //public string? Address { get; set; }
        //public DateTime? DateOfBirth { get; set; }
    }
}