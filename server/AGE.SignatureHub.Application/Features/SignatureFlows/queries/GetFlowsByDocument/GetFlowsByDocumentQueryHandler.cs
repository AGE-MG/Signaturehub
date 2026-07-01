using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetFlowsByDocument
{
    public class GetFlowsByDocumentQueryHandler : IRequestHandler<GetFlowsByDocumentQuery, BaseResponse<List<SignatureFlowDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetFlowsByDocumentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<SignatureFlowDto>>> Handle(GetFlowsByDocumentQuery request, CancellationToken cancellationToken)
        {
            var flows = await _unitOfWork.SignatureFlows.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
            return new BaseResponse<List<SignatureFlowDto>>
            {
                Success = true,
                Data = _mapper.Map<List<SignatureFlowDto>>(flows)
            };
        }
    }    
}
