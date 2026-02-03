using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGE.SignatureHub.Infrastructure.Persistence.Repositories
{
    public class AuditLogRepository : Repository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(al => al.Timestamp >= startDate && al.Timestamp <= endDate)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(al => al.DocumentId == documentId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetBySignerIdAsync(Guid signerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(al => al.SignerId == signerId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(al => al.UserId == userId)
            .OrderByDescending(al => al.Timestamp)
            .ToListAsync(cancellationToken);
        }
    }
}