using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Queries.GetSignerById
{
    public class GetSignerByIdQueryHandler : IRequestHandler<GetSignerByIdQuery, BaseResponse<SignerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetSignerByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<SignerDto>> Handle(GetSignerByIdQuery request, CancellationToken cancellationToken)
        {
            var signer = await _unitOfWork.Signers.GetByIdWithFlowAndDocumentAsync(request.SignerId, cancellationToken);
            if (signer == null)
            {
                throw new NotFoundException(nameof(Signer), request.SignerId);
            }

            var document = signer.SignatureFlow.Document;
            var normalizedEmail = (request.RequestingUserEmail ?? string.Empty).Trim().ToLowerInvariant();
            var normalizedDepartment = (request.RequestingUserDepartment ?? string.Empty).Trim().ToLowerInvariant();
            var hasAccess = string.Equals(signer.Email, request.RequestingUserEmail, StringComparison.OrdinalIgnoreCase) ||
                document.CreatedByUserId == request.RequestingUserId ||
                (!document.IsConfidential &&
                 !string.IsNullOrWhiteSpace(normalizedDepartment) &&
                 document.OwningDepartment.ToLowerInvariant() == normalizedDepartment) ||
                signer.ValidateInvitationToken(request.InvitationToken);

            if (!hasAccess)
            {
                throw new NotFoundException(nameof(Signer), request.SignerId);
            }

            return new BaseResponse<SignerDto>
            {
                Success = true,
                Data = _mapper.Map<SignerDto>(signer)
            };
        }
    }
}
