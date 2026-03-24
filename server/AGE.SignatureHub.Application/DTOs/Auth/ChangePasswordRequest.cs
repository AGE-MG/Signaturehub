using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class ChangePasswordRequest
    {    
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        
        public ChangePasswordRequest()
        {
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }
}