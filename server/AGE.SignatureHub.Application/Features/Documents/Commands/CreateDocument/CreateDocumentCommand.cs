using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<BaseResponse<DocumentDto>>
    {
        public Stream FileStream { get; set; } = null!;
        public string FileName { get; set; } = string.Empty;
        public CreateDocumentDto DocumentData { get; set; } = null!;
    }
}