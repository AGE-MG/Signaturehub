using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentByStatus
{
    public class GetDocumentByStatusQueryHandler : IRequestHandler<GetDocumentByStatusQuery, BaseResponse<List<DocumentDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDocumentByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<DocumentDto>>> Handle(GetDocumentByStatusQuery request, CancellationToken cancellationToken)
        {
            var documents = await _unitOfWork.Documents.GetAccessibleDocumentsAsync(
                request.RequestingUserId,
                request.RequestingUserEmail,
                request.RequestingUserDepartment,
                request.Status,
                cancellationToken);

            return new BaseResponse<List<DocumentDto>>
            {
                Success = true,
                Data = _mapper.Map<List<DocumentDto>>(documents)
            };
        }
    }
}
