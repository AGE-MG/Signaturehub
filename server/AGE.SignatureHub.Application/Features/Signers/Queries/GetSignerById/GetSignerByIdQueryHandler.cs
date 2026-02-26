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
            var response = new BaseResponse<SignerDto>();

            try
            {
                var signer = await _unitOfWork.Signers.GetByIdAsync(request.SignerId, cancellationToken);
                if (signer == null)
                {
                    throw new NotFoundException(nameof(Signer), request.SignerId);
                }
                
                response.Data = _mapper.Map<SignerDto>(signer);
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred while retrieving the signer: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}