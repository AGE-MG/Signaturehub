using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDateRange
{
    public class GetAuditLogsByDateRangeQueryHandler : IRequestHandler<GetAuditLogsByDateRangeQuery, BaseResponse<List<AuditLogDto>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public GetAuditLogsByDateRangeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BaseResponse<List<AuditLogDto>>> Handle(GetAuditLogsByDateRangeQuery request, CancellationToken cancellationToken)
        {
            var auditLogs = await _unitOfWork.AuditLogs.GetByDateRangeAsync(
                request.StartDate,
                request.EndDate,
                cancellationToken
            );

            if (!request.IsAdmin)
            {
                var accessibleDocuments = await _unitOfWork.Documents.GetAccessibleDocumentsAsync(
                    request.RequestingUserId,
                    request.RequestingUserEmail,
                    request.RequestingUserDepartment,
                    status: null,
                    cancellationToken: cancellationToken);
                var accessibleDocumentIds = accessibleDocuments.Select(d => d.Id).ToHashSet();

                auditLogs = auditLogs
                    .Where(log => log.UserId == request.RequestingUserId ||
                        (log.DocumentId.HasValue && accessibleDocumentIds.Contains(log.DocumentId.Value)))
                    .ToList();
            }

            return new BaseResponse<List<AuditLogDto>>
            {
                Success = true,
                Data = _mapper.Map<List<AuditLogDto>>(auditLogs)
            };
        }
    }
}
