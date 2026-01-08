using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public interface IEmailService
    {
        Task SendSignatureRequestAsync(string toEmail, string toName, string documentTitle, string signatureUrl, CancellationToken cancellationToken = default);
        Task SendSignatureCompletedAsync(string toEmail, string toName, string documentTitle, CancellationToken cancellationToken = default);
        Task SendSignatureRejectedAsync(string toEmail, string toName, string documentTitle, string reason, CancellationToken cancellationToken = default);
        Task SendDocumentExpiredAsync(string toEmail, string toName, string documentTitle, CancellationToken cancellationToken = default);
        Task SendReminderAsync(string toEmail, string toName, string documentTitle, string signatureUrl, CancellationToken cancellationToken = default);
    }
}