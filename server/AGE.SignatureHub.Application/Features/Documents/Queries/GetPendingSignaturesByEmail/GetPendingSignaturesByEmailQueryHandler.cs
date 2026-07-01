using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetPendingSignaturesByEmail
{
    public class GetPendingSignaturesByEmailQueryHandler : IRequestHandler<GetPendingSignaturesByEmailQuery, BaseResponse<List<SignerDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendingSignaturesByEmailQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        public async Task<BaseResponse<List<SignerDto>>> Handle(GetPendingSignaturesByEmailQuery request, CancellationToken cancellationToken)
        {
            var signers = await _unitOfWork.Signers.GetPendingSignersByEmailAsync(request.Email);
            if (signers != null && signers.Any())
            {
                return new BaseResponse<List<SignerDto>>
                {
                    Success = true,
                    Message = "Pending signatures retrieved successfully.",
                    Data = _mapper.Map<List<SignerDto>>(signers)
                };
            }

            return new BaseResponse<List<SignerDto>>
            {
                Success = false,
                Message = "No pending signatures found for the provided email.",
                Data = new List<SignerDto>()
            };
        }
    }
}
