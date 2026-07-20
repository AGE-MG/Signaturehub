using AGE.SignatureHub.Domain.Common;

namespace AGE.SignatureHub.Domain.Entities;

public sealed class ExternalServiceConnection : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Secret { get; private set; } = string.Empty;
    public string EventsCsv { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastDeliveryAt { get; private set; }
    public bool? LastDeliverySucceeded { get; private set; }

    private ExternalServiceConnection() { }

    public ExternalServiceConnection(Guid userId, string name, string url, string secret, IEnumerable<string> events)
    {
        UserId = userId;
        Update(name, url, events);
        Secret = secret;
    }

    public IReadOnlyList<string> Events => EventsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    public void Update(string name, string url, IEnumerable<string> events)
    {
        Name = name.Trim();
        Url = url.Trim();
        EventsCsv = string.Join(',', events.Select(value => value.Trim().ToLowerInvariant()).Distinct());
        SetUpdatedAt();
    }

    public void SetActive(bool active) { IsActive = active; SetUpdatedAt(); }
    public void RegisterDelivery(bool succeeded) { LastDeliveryAt = DateTime.UtcNow; LastDeliverySucceeded = succeeded; SetUpdatedAt(); }
}
