using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<Document?> GetByIdWithFlowsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Document?> GetByIdWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Document?> GetByIdWithAllRelationsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Document>> GetByStatusAsync(DocumentStatus status, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Document>> GetByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Document>> GetExpiredDocumentsAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Document>> GetPendingDocumentsAsync(CancellationToken cancellationToken = default);
    }
}