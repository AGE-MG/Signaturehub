using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.DTOs.Notifications;
using Hangfire;

namespace AGE.SignatureHub.Infrastructure.Services.Notifications;

public sealed class DocumentNotificationDispatcher : IDocumentNotificationDispatcher
{
    private readonly IBackgroundJobClient _jobs;

    public DocumentNotificationDispatcher(IBackgroundJobClient jobs) => _jobs = jobs;

    public Task EnqueueAsync(DocumentEventNotification notification, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _jobs.Enqueue<DocumentNotificationJob>(job => job.ProcessAsync(notification, CancellationToken.None));
        return Task.CompletedTask;
    }
}
