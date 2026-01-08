using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface ISignatureFlowRepository : IRepository<SignatureFlow>
    {
        Task<SignatureFlow?> GetByIdWithSignersAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SignatureFlow>> GetByDocumentIdAsync(Guid documentId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<SignatureFlow>> GetIncompleteFlowsAsync(CancellationToken cancellationToken = default);
    }
}