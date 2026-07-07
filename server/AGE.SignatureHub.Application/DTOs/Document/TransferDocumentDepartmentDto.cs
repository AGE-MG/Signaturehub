using System;

namespace AGE.SignatureHub.Application.DTOs.Document
{
    public class TransferDocumentDepartmentDto
    {
        public Guid TargetUserId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid RequestingUserId { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
