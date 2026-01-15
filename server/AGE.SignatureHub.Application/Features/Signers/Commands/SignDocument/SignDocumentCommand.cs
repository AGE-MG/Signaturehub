using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Commands.SignDocument
{
    public class SignDocumentCommand : IRequest<BaseResponse<SignerDto>>
    {
        public SignDocumentDto SignData { get; set; } = null!;
    }
}