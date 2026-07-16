namespace AGE.SignatureHub.Application.DTOs.Notifications;

public sealed class DocumentEventNotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventType { get; init; } = string.Empty;
    public Guid DocumentId { get; init; }
    public string DocumentTitle { get; init; } = string.Empty;
    public Guid ActorUserId { get; init; }
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public string? Details { get; init; }
    public IReadOnlyCollection<string> RecipientEmails { get; init; } = Array.Empty<string>();
}
