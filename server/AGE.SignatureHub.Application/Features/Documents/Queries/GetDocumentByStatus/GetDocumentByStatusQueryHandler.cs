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
            var response = new BaseResponse<List<DocumentDto>>();

            try
            {
                var Documents = await _unitOfWork.Documents.GetByStatusAsync(request.Status);
                response.Success = true;
                response.Data = _mapper.Map<List<DocumentDto>>(Documents);

                return response;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = "Error retrieving documents by status.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}