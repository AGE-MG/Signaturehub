using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetSignatureFlowById
{
    public class GetSignatureFlowByIdQuery : IRequest<BaseResponse<SignatureFlowDto>>
    {
        public Guid Id { get; set; }
    }
}