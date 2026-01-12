using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;

namespace AGE.SignatureHub.Application.DTOs.Signer
{
    public class SignerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Document { get; set; } = string.Empty;
        public SignerRole Role { get; set; }
        public int SignOrder { get; set; }
        public SignatureStatus Status { get; set; }
        public SignatureType? SignatureType { get; set; }
        public DateTime? SignedAt { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public CertificateInfo CertificateInfo { get; set; } = null!;
    }
}