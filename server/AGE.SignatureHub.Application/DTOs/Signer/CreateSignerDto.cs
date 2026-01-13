using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.Signer
{
    public class CreateSignerDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public int SignOrder { get; set; }
        public SignerRole Role { get; set; }
    }
}