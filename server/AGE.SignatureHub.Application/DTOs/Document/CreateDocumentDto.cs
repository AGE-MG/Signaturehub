using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.DTOs.Document
{
    public class CreateDocumentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public Guid CreatedByUserId { get; set; }
    }
}