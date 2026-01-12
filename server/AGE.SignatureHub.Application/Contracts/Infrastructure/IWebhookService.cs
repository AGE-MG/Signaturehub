using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public interface IWebhookService
    {
        Task SendWebhookAsync(
            string eventType,
            string payload,
            CancellationToken cancellationToken = default);
    }
}