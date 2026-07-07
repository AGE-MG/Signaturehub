using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.DownloadDocument
{
    public class DownloadDocumentQuery : IRequest<DownloadDocumentResponse>
    {
        public Guid DocumentId { get; set; }
        public int? VersionNumber { get; set; }
        public Guid RequestingUserId { get; set; }
        public string RequestingUserEmail { get; set; } = string.Empty;
        public string? RequestingUserDepartment { get; set; }
    }

    public class DownloadDocumentResponse
    {
        public Stream FileStream { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
