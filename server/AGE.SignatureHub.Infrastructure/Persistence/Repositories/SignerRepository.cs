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
    public class SignerRepository : Repository<Signer>, ISignerRepository
    {
        public SignerRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<Signer>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(s => s.SignatureFlow)
            .ThenInclude(sf => sf.Document)
            .Where(s => s.Email == email)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Signer>> GetByFlowIdAsync(Guid flowId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Where(s => s.SignatureFlowId == flowId)
            .OrderBy(s => s.SignOrder)
            .ToListAsync(cancellationToken);
        }

        public async Task<Signer?> GetByIdWithFlowAndDocumentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(s => s.SignatureFlow)
            .ThenInclude(sf => sf.Document)
            .Include(s => s.SignatureFlow)
            .ThenInclude(sf => sf.Signers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Signer>> GetPendingSignersByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(s => s.SignatureFlow)
            .ThenInclude(sf => sf.Document)
            .Where(s => s.Email == email && s.Status == SignatureStatus.Pending)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
        }
    }
}