using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQuery : IRequest<BaseResponse<DocumentDto>>
    {
        public Guid DocumentId { get; set; }
        public Guid RequestingUserId { get; set; }
        public string RequestingUserEmail { get; set; } = string.Empty;
        public string? RequestingUserDepartment { get; set; }
    }
}
