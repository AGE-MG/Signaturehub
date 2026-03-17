using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public RefreshTokenRequest()
        {
            Token = string.Empty;
            RefreshToken = string.Empty;
        }
    }
}