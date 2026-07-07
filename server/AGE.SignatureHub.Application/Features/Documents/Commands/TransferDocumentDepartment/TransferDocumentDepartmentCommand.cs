using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.TransferDocumentDepartment
{
    public class TransferDocumentDepartmentCommand : IRequest<BaseResponse<DocumentDto>>
    {
        public Guid DocumentId { get; set; }
        public TransferDocumentDepartmentDto TransferData { get; set; } = new();
    }
}
