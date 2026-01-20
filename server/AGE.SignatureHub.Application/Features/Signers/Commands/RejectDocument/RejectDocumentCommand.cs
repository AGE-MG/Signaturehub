using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Commands.RejectDocument
{
    public class RejectDocumentCommand : IRequest<BaseResponse<SignerDto>>
    {
        public RejectDocumentDto RejectData { get; set; } = new RejectDocumentDto();
    }
}