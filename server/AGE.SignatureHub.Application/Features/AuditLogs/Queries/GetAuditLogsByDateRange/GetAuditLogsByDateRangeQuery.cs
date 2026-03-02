using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using MediatR;

namespace AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDateRange
{
    public class GetAuditLogsByDateRangeQuery : IRequest<BaseResponse<List<AuditLogDto>>>
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}