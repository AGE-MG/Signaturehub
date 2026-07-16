using System.Text.Json;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Notifications;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Services.Notifications;

public sealed class DocumentNotificationJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IEmailService _email;
    private readonly IWebhookService _webhooks;
    private readonly ILogger<DocumentNotificationJob> _logger;

    public DocumentNotificationJob(IUnitOfWork unitOfWork, UserManager<ApplicationUser> users,
        IEmailService email, IWebhookService webhooks, ILogger<DocumentNotificationJob> logger)
    {
        _unitOfWork = unitOfWork;
        _users = users;
        _email = email;
        _webhooks = webhooks;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ProcessAsync(DocumentEventNotification message, CancellationToken cancellationToken)
    {
        var recipients = await ResolveRecipientsAsync(message, cancellationToken);
        var (title, body, type) = Describe(message);

        foreach (var recipient in recipients)
        {
            Guid? relatedDocumentId = message.EventType == "document.deleted" ? null : message.DocumentId;
            await _unitOfWork.Notifications.AddAsync(
                new Notification(recipient.Id, title, body, type, relatedDocumentId), cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // External channels are deliberately isolated from the durable in-app notification.
        await TryWebhookAsync(message, cancellationToken);
        foreach (var recipient in recipients.Where(user => !string.IsNullOrWhiteSpace(user.Email)))
        {
            await TryEmailAsync(recipient, title, body, cancellationToken);
        }
    }

    private async Task<List<ApplicationUser>> ResolveRecipientsAsync(DocumentEventNotification message, CancellationToken cancellationToken)
    {
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var email in message.RecipientEmails)
            if (!string.IsNullOrWhiteSpace(email)) emails.Add(email.Trim());

        if (message.EventType != "document.deleted")
        {
            var document = await _unitOfWork.Documents.GetByIdWithAllRelationsAsync(message.DocumentId, cancellationToken);
            if (document is not null)
            {
                foreach (var email in document.SignatureFlows.SelectMany(flow => flow.Signers).Select(signer => signer.Email))
                    if (!string.IsNullOrWhiteSpace(email)) emails.Add(email.Trim());
            }
        }

        return await _users.Users
            .Where(user => user.Id == message.ActorUserId || (user.Email != null && emails.Contains(user.Email)))
            .ToListAsync(cancellationToken);
    }

    private async Task TryWebhookAsync(DocumentEventNotification message, CancellationToken cancellationToken)
    {
        try { await _webhooks.SendWebhookAsync(message.EventType, JsonSerializer.Serialize(message), cancellationToken); }
        catch (Exception ex) { _logger.LogWarning(ex, "Non-critical webhook delivery failed for event {EventId}", message.EventId); }
    }

    private async Task TryEmailAsync(ApplicationUser recipient, string subject, string body, CancellationToken cancellationToken)
    {
        try { await _email.SendDocumentEventAsync(recipient.Email!, recipient.FullName, subject, body, cancellationToken); }
        catch (Exception ex) { _logger.LogWarning(ex, "Non-critical email delivery failed for event recipient {UserId}", recipient.Id); }
    }

    private static (string Title, string Body, NotificationType Type) Describe(DocumentEventNotification message) => message.EventType switch
    {
        "document.created" => ("Novo documento", $"O documento '{message.DocumentTitle}' foi criado.", NotificationType.DocumentCreated),
        "document.completed" => ("Documento concluído", $"O documento '{message.DocumentTitle}' foi concluído.", NotificationType.DocumentCompleted),
        "document.deleted" => ("Documento removido", $"O documento '{message.DocumentTitle}' foi removido.", NotificationType.DocumentDeleted),
        _ => ("Documento atualizado", $"O documento '{message.DocumentTitle}' foi atualizado.{FormatDetails(message.Details)}", NotificationType.DocumentUpdated)
    };

    private static string FormatDetails(string? details) => string.IsNullOrWhiteSpace(details) ? string.Empty : $" {details.Trim()}";
}
