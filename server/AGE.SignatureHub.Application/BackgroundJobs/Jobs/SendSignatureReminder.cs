using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Configuration;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Application.BackgroundJobs.Jobs
{
    public class SendSignatureReminder
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendSignatureReminder> _logger;
        private readonly ApplicationSettings _settings;

        public SendSignatureReminder(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<SendSignatureReminder> logger, ApplicationSettings settings)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
            _settings = settings;
        }

        public async Task ExecuteAsync(Guid signerId)
        {
            try
            {
                _logger.LogInformation("Starting signature reminder job for signerId: {SignerId}", signerId);

                var signer = await _unitOfWork.Signers.GetByIdWithFlowAndDocumentAsync(signerId);
                if (signer == null)
                {
                    _logger.LogWarning("Signer with ID {SignerId} not found.", signerId);
                    return;
                }

                if (signer.Status != SignatureStatus.Pending)
                {
                    _logger.LogInformation("Signer with ID {SignerId} is not pending. Current status: {Status}", signerId, signer.Status);
                    return;
                }

                var document = signer.SignatureFlow.Document;

                var signatureUrl = $"{_settings.BaseUrl}/{_settings.SignatureUrlPath}/{signer.Id}";

                await _emailService.SendReminderAsync(signer.Email,signer.Name, document.Title, signatureUrl);

                _logger.LogInformation("Signature reminder email sent to {Email} for document '{DocumentTitle}'.", signer.Email, document.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing the signature reminder job for signerId: {SignerId}", signerId);
                throw;
            }
        }
    }
}