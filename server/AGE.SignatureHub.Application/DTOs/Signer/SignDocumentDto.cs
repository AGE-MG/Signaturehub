using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.Signer
{
    public class SignDocumentDto
    {
        public Guid SignerId { get; set; }
        public SignatureType SignatureType { get; set; }
        public byte[] CertificateData { get; set; } = Array.Empty<byte>();
        public string Pin { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}