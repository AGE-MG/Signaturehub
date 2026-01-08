using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface ISignerRepository : IRepository<Signer>
    {
        Task<IReadOnlyList<Signer>> GetByFlowIdAsync(Guid flowId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Signer>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Signer>> GetPendingSignersByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Signer?> GetByIdWithFlowAndDocumentAsync(Guid id, CancellationToken cancellationToken = default);
    }
}