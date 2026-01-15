using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.Commands.CreateSignatureFlow
{
    public class CreateSignatureFlowCommand : IRequest<BaseResponse<SignatureFlowDto>>
    {
        public CreateSignatureFlowDto FlowData { get; set; } = null!;
    }
}
