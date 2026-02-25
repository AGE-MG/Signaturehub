using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetFlowsByDocument
{
    public class GetFlowsByDocumentQuery : IRequest<BaseResponse<List<SignatureFlowDto>>>
    {
        public Guid DocumentId { get; set; }
    }
}