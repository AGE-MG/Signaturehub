using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using AGE.SignatureHub.Application.Exceptions;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetSignatureFlowById
{
    public class GetSignatureFlowByIdQueryHandler : IRequestHandler<GetSignatureFlowByIdQuery, BaseResponse<SignatureFlowDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSignatureFlowByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<SignatureFlowDto>> Handle(GetSignatureFlowByIdQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<SignatureFlowDto>();

            try
            {
                var signatureFlow = await _unitOfWork.SignatureFlows.GetByIdWithSignersAsync(request.Id);

                if (signatureFlow == null)
                {
                    throw new NotFoundException(nameof(SignatureFlows), request.Id);
                }

                response.Data = _mapper.Map<SignatureFlowDto>(signatureFlow);
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while retrieving the signature flow: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }

        }
    }
}