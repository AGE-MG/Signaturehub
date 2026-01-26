using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Application.BackgroundJobs
{
    public class CheckExpiredBackgroundJobs
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckExpiredBackgroundJobs> _logger;

        public CheckExpiredBackgroundJobs(IUnitOfWork unitOfWork, IEmailService emailService, ILogger<CheckExpiredBackgroundJobs> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                _logger.LogInformation("Starting CheckExpiredBackgroundJobs...");
    
                var expiredDocuments = await _unitOfWork.Documents.GetExpiredDocumentsAsync();
    
                foreach (var document in expiredDocuments)
                {
                    document.CheckAndUpdateExpiration();
                    
                    if (document.Status == DocumentStatus.Expired)
                    {
                        await _unitOfWork.Documents.UpdateAsync(document);

                        var flows = await _unitOfWork.SignatureFlows.GetByDocumentIdAsync(document.Id);
                        foreach (var flow in flows)
                        {
                            var pendingSigners = flow.Signers.Where(s => s.Status == SignatureStatus.Pending).ToList();

                            foreach (var signer in pendingSigners)
                            {
                                signer.Cancel();
                                await _unitOfWork.Signers.UpdateAsync(signer);

                                await _emailService.SendDocumentExpiredAsync(signer.Email, signer.Name, document.Title);
                            }
                        }

                        var auditLog = new AuditLog(
                            action: "DOCUMENT_EXPIRED",
                            details: $"Document '{document.Title}' has expired automatically.",
                            ipAddress: "System",
                            userAgent: "BackgroundJob",
                            documentId: document.Id
                        );
                        await _unitOfWork.AuditLogs.AddAsync(auditLog);
                    }
                }
    
                await _unitOfWork.SaveChangesAsync();
    
                _logger.LogInformation("Expired documents check job completed. Processed {Count} documents.", expiredDocuments.Count);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing CheckExpiredBackgroundJobs.");
                throw;
            }
        }
    }
}