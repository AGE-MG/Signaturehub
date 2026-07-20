using AGE.SignatureHub.Application.DTOs.Notifications;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure;

public interface IExternalServiceConnectionService
{
    Task<IReadOnlyList<ExternalServiceConnectionDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ExternalServiceConnectionDto> CreateAsync(Guid userId, SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken = default);
    Task<ExternalServiceConnectionDto?> UpdateAsync(Guid userId, Guid id, SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken = default);
    Task<bool> SetActiveAsync(Guid userId, Guid id, bool active, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);
    Task DispatchAsync(Guid userId, string eventType, string payload, CancellationToken cancellationToken = default);
}
