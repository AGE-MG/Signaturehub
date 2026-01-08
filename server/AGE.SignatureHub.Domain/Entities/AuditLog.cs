using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Common;

namespace AGE.SignatureHub.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public Guid? DocumentId { get; private set; }
        public Guid? SignerId { get; private set; }
        public Guid? UserId { get; private set; }
        public string Action { get; private set; }
        public string Details { get; private set; }
        public string IpAddress { get; private set; }
        public string UserAgent { get; private set; }
        public DateTime Timestamp { get; private set; }

        private AuditLog() { }

        public AuditLog(
                string action,
                string details,
                string ipAddress,
                string userAgent,
                Guid? documentId = null,
                Guid? signerId = null,
                Guid? userId = null
            )
        {
            DocumentId = documentId;
            SignerId = signerId;
            UserId = userId;
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Details = details;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Timestamp = DateTime.UtcNow;
        }
    }
}