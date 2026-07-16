using AGE.SignatureHub.Application.DTOs.Notifications;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure;

public interface IDocumentNotificationDispatcher
{
    Task EnqueueAsync(DocumentEventNotification notification, CancellationToken cancellationToken = default);
}
