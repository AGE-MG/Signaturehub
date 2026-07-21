using System;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.Commands.TransferSignatureResponsibility
{
    public class TransferSignatureResponsibilityCommand : IRequest<BaseResponse<DocumentDto>>
    {
        public Guid DocumentId { get; set; }
        public TransferSignatureResponsibilityDto TransferData { get; set; } = new();
    }
}
