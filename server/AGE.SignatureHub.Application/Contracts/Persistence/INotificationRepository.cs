using AGE.SignatureHub.Domain.Entities;

namespace AGE.SignatureHub.Application.Contracts.Persistence
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<IReadOnlyList<Notification>> GetByUserIdAsync(Guid userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
        Task<int> CountUnreadByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
