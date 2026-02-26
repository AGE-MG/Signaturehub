using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Queries.GetSignerById
{
    public class GetSignerByIdQuery : IRequest<BaseResponse<SignerDto>>
    {
        public Guid SignerId { get; set; }
    }
}