using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }

        public LoginRequest()
        {
            Email = string.Empty;
            Password = string.Empty;
            RememberMe = false;
        }
    }
}