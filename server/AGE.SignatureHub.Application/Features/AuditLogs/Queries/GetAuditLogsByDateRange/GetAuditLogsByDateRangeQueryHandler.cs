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
            var response = new BaseResponse<List<AuditLogDto>>();

            try
            {
                var auditLogs = await _unitOfWork.AuditLogs.GetByDateRangeAsync(
                    request.StartDate,
                    request.EndDate,
                    cancellationToken
                );

                response.Data = _mapper.Map<List<AuditLogDto>>(auditLogs);
                response.Success = true;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while retrieving audit logs.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }
    }
}