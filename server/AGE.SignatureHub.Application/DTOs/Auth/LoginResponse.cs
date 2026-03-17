using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public DateTime TokenExpiration { get; set; }
        public UserDto User { get; set; }
        public LoginResponse()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
            User = new UserDto();
        }
    }
}