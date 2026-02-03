using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AGE.SignatureHub.Infrastructure.Persistence.Repositories
{
    public class SignatureFlowRepository : Repository<SignatureFlow>, ISignatureFlowRepository
    {
        public SignatureFlowRepository(ApplicationDBContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<SignatureFlow>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(sf => sf.Signers.OrderBy(s => s.SignOrder))
            .Where(sf => sf.DocumentId == documentId)
            .ToListAsync(cancellationToken);
        }

        public async Task<SignatureFlow?> GetByIdWithSignersAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(sf => sf.Signers.OrderBy(s => s.SignOrder))
            .Include(sf => sf.Document)
            .FirstOrDefaultAsync(sf => sf.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<SignatureFlow>> GetIncompleteFlowsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
            .Include(sf => sf.Signers)
            .Include(sf => sf.Document)
            .Where(sf => !sf.IsCompleted)
            .ToListAsync(cancellationToken);
        }
    }
}