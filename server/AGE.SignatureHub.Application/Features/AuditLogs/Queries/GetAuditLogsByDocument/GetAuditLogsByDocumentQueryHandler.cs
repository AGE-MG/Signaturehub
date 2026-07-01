using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AutoMapper;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDocument
{
    public class GetAuditLogsByDocumentQueryHandler : IRequestHandler<GetAuditLogsByDocumentQuery, BaseResponse<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetAuditLogsByDocumentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<AuditLogDto>>> Handle(GetAuditLogsByDocumentQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _unitOfWork.AuditLogs.GetByDocumentIdAsync(request.DocumentId);
            return new BaseResponse<List<AuditLogDto>>
            {
                Success = true,
                Data = _mapper.Map<List<AuditLogDto>>(auditLogs)
            };
        }
    }
}
