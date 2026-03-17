using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? ProfilePicture { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? RegistrationNumber { get; set; }
        public List<string> Roles { get; set; }
        public UserDto()
        {
            FullName = string.Empty;
            Email = string.Empty;
            Roles = new List<string>();
        }
    }
}