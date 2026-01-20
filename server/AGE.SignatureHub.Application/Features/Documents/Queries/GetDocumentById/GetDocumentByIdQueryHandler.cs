using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Exceptions;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, BaseResponse<DocumentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDocumentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<DocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<DocumentDto>();

            try
            {
                var document = await _unitOfWork.Documents.GetByIdWithAllRelationsAsync(request.DocumentId, cancellationToken);
                if (document == null)
                {
                    throw new NotFoundException(nameof(document), request.DocumentId);
                }

                response.Data = _mapper.Map<DocumentDto>(document);
                response.Success = true;
                return response;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving the document.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }

        }
    }
}