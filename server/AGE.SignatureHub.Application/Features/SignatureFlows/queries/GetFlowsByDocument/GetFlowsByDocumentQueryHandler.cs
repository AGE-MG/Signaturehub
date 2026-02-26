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
            var response = new BaseResponse<List<SignatureFlowDto>>();

            try
            {
                var flows = await _unitOfWork.SignatureFlows.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
                response.Data = _mapper.Map<List<SignatureFlowDto>>(flows);
                response.Success = true;
                return response;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while retrieving the signature flows: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }    
}