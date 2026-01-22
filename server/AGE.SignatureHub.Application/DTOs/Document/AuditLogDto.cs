using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Document
{
    public class AuditLogDto
    {
        public Guid Id { get; set; }
        public Guid DocumentId { get; set; }
        public Guid? SignerId { get; set; }
        public Guid? UserId { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Timestamp { get; set; }

        public AuditLogDto(string action, string details, string ipAddress, string userAgent)
        {
            Action = action;
            Details = details;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Timestamp = DateTime.UtcNow;
        }
    }
}