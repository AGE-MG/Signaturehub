namespace AGE.SignatureHub.Application.DTOs.Notifications;

public sealed class ExternalServiceConnectionDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public IReadOnlyList<string> Events { get; init; } = [];
    public bool IsActive { get; init; }
    public DateTime? LastDeliveryAt { get; init; }
    public bool? LastDeliverySucceeded { get; init; }
    public string? Secret { get; init; }
}

public sealed class SaveExternalServiceConnectionRequest
{
    public string Name { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public IReadOnlyList<string> Events { get; init; } = [];
}
