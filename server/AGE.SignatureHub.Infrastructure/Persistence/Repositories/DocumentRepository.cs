using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AGE.SignatureHub.Infrastructure.Persistence.Repositories
{
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {

        public DocumentRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Document>> GetByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(d => d.CreatedByUserId == creatorId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
        }

        public async Task<Document?> GetByIdWithAllRelationsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(d => d.SignatureFlows)
            .ThenInclude(sf => sf.Signers)
            .Include(d => d.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<Document?> GetByIdWithFlowsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(d => d.SignatureFlows)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<Document?> GetByIdWithVersionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(d => d.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Document>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(d => d.Status == status)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            return await _dbSet
            .Where(d => d.ExpiresAt.HasValue && d.ExpiresAt <= now && d.Status != DocumentStatus.Completed && d.Status != DocumentStatus.Cancelled && d.Status != DocumentStatus.Expired)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Document>> GetPendingDocumentsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(d => d.Status == DocumentStatus.PendingSignatures || d.Status == DocumentStatus.PartiallyCompleted)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync(cancellationToken);
        }
    }
}