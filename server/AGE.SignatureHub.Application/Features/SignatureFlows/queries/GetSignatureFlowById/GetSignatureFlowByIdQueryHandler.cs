using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
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
            var signatureFlow = await _unitOfWork.SignatureFlows.GetByIdWithSignersAsync(request.Id);

            if (signatureFlow == null)
            {
                throw new NotFoundException(nameof(SignatureFlow), request.Id);
            }

            var document = signatureFlow.Document;
            var normalizedEmail = (request.RequestingUserEmail ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedDepartment = (request.RequestingUserDepartment ?? string.Empty).Trim().ToLowerInvariant();
            var hasAccess = document.CreatedByUserId == request.RequestingUserId ||
                signatureFlow.Signers.Any(s => s.Email.ToLowerInvariant() == normalizedEmail) ||
                (!document.IsConfidential &&
                 !string.IsNullOrWhiteSpace(normalizedDepartment) &&
                 document.OwningDepartment.ToLowerInvariant() == normalizedDepartment);

            if (!hasAccess)
            {
                throw new NotFoundException(nameof(SignatureFlow), request.Id);
            }

            return new BaseResponse<SignatureFlowDto>
            {
                Success = true,
                Data = _mapper.Map<SignatureFlowDto>(signatureFlow)
            };
        }
    }
}
