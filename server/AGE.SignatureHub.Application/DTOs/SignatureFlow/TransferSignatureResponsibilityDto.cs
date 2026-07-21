using System;

namespace AGE.SignatureHub.Application.DTOs.SignatureFlow
{
    public class TransferSignatureResponsibilityDto
    {
        public string NewResponsibleName { get; set; } = string.Empty;
        public string NewResponsibleEmail { get; set; } = string.Empty;
        public string NewResponsibleDocument { get; set; } = string.Empty;
        public Guid RequestingUserId { get; set; }
        public string RequestingUserEmail { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
