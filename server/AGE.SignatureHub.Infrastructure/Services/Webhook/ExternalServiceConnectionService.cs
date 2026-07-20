using System.Net;
using System.Security.Cryptography;
using System.Text;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.DTOs.Notifications;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Services.Webhook;

public sealed class ExternalServiceConnectionService : IExternalServiceConnectionService
{
    public static readonly string[] AllowedEvents =
    [
        "document.created", "document.updated", "document.completed", "document.deleted",
        "signature.requested", "signature.signed", "signature.rejected"
    ];

    private readonly ApplicationDBContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalServiceConnectionService> _logger;

    public ExternalServiceConnectionService(ApplicationDBContext dbContext, HttpClient httpClient, ILogger<ExternalServiceConnectionService> logger)
    {
        _dbContext = dbContext;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ExternalServiceConnectionDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var connections = await _dbContext.ExternalServiceConnections.AsNoTracking()
            .Where(connection => connection.UserId == userId)
            .OrderBy(connection => connection.Name)
            .ToListAsync(cancellationToken);
        return connections.Select(connection => Map(connection, null)).ToList();
    }

    public async Task<ExternalServiceConnectionDto> CreateAsync(Guid userId, SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var events = Validate(request);
        if (await _dbContext.ExternalServiceConnections.AnyAsync(connection => connection.UserId == userId && connection.Name == request.Name.Trim(), cancellationToken))
            throw new BusinessException("Já existe uma integração com esse nome.");
        var secret = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var connection = new ExternalServiceConnection(userId, request.Name, request.Url, secret, events);
        _dbContext.ExternalServiceConnections.Add(connection);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(connection, secret);
    }

    public async Task<ExternalServiceConnectionDto?> UpdateAsync(Guid userId, Guid id, SaveExternalServiceConnectionRequest request, CancellationToken cancellationToken = default)
    {
        var connection = await FindOwnedAsync(userId, id, cancellationToken);
        if (connection is null) return null;
        if (await _dbContext.ExternalServiceConnections.AnyAsync(item => item.UserId == userId && item.Id != id && item.Name == request.Name.Trim(), cancellationToken))
            throw new BusinessException("Já existe uma integração com esse nome.");
        connection.Update(request.Name, request.Url, Validate(request));
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(connection, null);
    }

    public async Task<bool> SetActiveAsync(Guid userId, Guid id, bool active, CancellationToken cancellationToken = default)
    {
        var connection = await FindOwnedAsync(userId, id, cancellationToken);
        if (connection is null) return false;
        connection.SetActive(active);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var connection = await FindOwnedAsync(userId, id, cancellationToken);
        if (connection is null) return false;
        connection.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task DispatchAsync(Guid userId, string eventType, string payload, CancellationToken cancellationToken = default)
    {
        var candidates = await _dbContext.ExternalServiceConnections
            .Where(connection => connection.UserId == userId && connection.IsActive)
            .ToListAsync(cancellationToken);
        var connections = candidates.Where(connection => connection.Events.Contains(eventType, StringComparer.OrdinalIgnoreCase)).ToList();

        foreach (var connection in connections)
        {
            var succeeded = await SendAsync(connection, eventType, payload, cancellationToken);
            connection.RegisterDelivery(succeeded);
        }
        if (connections.Count > 0) await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> SendAsync(ExternalServiceConnection connection, string eventType, string payload, CancellationToken cancellationToken)
    {
        try
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            using var request = new HttpRequestMessage(HttpMethod.Post, connection.Url)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-SignatureHub-Event", eventType);
            request.Headers.Add("X-SignatureHub-Timestamp", timestamp);
            request.Headers.Add("X-SignatureHub-Signature", Sign($"{timestamp}.{payload}", connection.Secret));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "User webhook {ConnectionId} delivery failed", connection.Id);
            return false;
        }
    }

    private async Task<ExternalServiceConnection?> FindOwnedAsync(Guid userId, Guid id, CancellationToken cancellationToken) =>
        await _dbContext.ExternalServiceConnections.FirstOrDefaultAsync(connection => connection.Id == id && connection.UserId == userId, cancellationToken);

    private static IReadOnlyList<string> Validate(SaveExternalServiceConnectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 100) throw new BusinessException("Nome da integração inválido.");
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || !string.IsNullOrEmpty(uri.UserInfo))
            throw new BusinessException("A URL deve ser HTTPS e não pode conter credenciais.");
        if (uri.IsLoopback || uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || IPAddress.TryParse(uri.Host, out var ip) && IPAddress.IsLoopback(ip))
            throw new BusinessException("Endereços locais não são permitidos.");
        var events = request.Events.Select(value => value.Trim().ToLowerInvariant()).Distinct().ToArray();
        if (events.Length == 0 || events.Any(value => !AllowedEvents.Contains(value))) throw new BusinessException("Selecione ao menos um evento válido.");
        return events;
    }

    private static string Sign(string value, string secret) =>
        Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static ExternalServiceConnectionDto Map(ExternalServiceConnection connection, string? secret) => new()
    {
        Id = connection.Id, Name = connection.Name, Url = connection.Url, Events = connection.Events,
        IsActive = connection.IsActive, LastDeliveryAt = connection.LastDeliveryAt,
        LastDeliverySucceeded = connection.LastDeliverySucceeded, Secret = secret
    };
}
