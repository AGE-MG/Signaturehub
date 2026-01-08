using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IReadOnlyList<AuditLog>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetBySignerIdAsync(Guid signerId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    }
}