using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Signer
{
    public class RejectDocumentDto
    {
        public Guid SignerId { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}