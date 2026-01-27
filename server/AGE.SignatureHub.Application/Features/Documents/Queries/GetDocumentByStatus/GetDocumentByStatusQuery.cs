using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Domain.Enums;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentByStatus
{
    public class GetDocumentByStatusQuery : IRequest<BaseResponse<List<DocumentDto>>>
    {
        public DocumentStatus Status { get; set; }
    }
}