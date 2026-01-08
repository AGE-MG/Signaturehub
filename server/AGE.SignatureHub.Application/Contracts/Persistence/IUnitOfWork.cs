using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IDocumentRepository Documents { get; }
        ISignatureFlowRepository SignatureFlows { get; }
        ISignerRepository Signers { get; }
        IAuditLogRepository AuditLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    }
}