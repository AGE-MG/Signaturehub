using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.BackgroundJobs;
using AGE.SignatureHub.Application.BackgroundJobs.Jobs;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Services.BackgroundJobs
{
    public class BackgroundJobService : IBackgroundJobsService
    {
        private readonly ILogger<BackgroundJobService> _logger;

        public BackgroundJobService(ILogger<BackgroundJobService> logger)
        {
            _logger = logger;
        }

        public void CheckeExpiredDocuments()
        {
            try
            {
                RecurringJob.AddOrUpdate<CheckExpiredDocumentJob>("check-ecpired-documents",
                    job => job.ExecuteAsync(), Cron.Daily);
                _logger.LogInformation("Scheduled recurring job for checking expired documents.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule recurring job for checking expired documents.");
                throw;
            }
        }

        public void CleanupOldAuditLogs(int daysToKeep)
        {
            try
            {
                RecurringJob.AddOrUpdate<CleanupOldAuditLogsJob>("cleanup-old-audit-logs",
                    job => job.ExecuteAsync(daysToKeep), Cron.Daily);
                _logger.LogInformation("Scheduled recurring job for cleaning up old audit logs.");
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule recurring job for cleaning up old audit logs.");
                throw;
            }
        }

        public void ScheduleSignatureReminder(Guid signerId, DateTime scheduledTime)
        {
            try
            {
                BackgroundJob.Schedule<SendSignatureReminder>(
                    job => job.ExecuteAsync(signerId),
                    scheduledTime
                );

                _logger.LogInformation(
                    "Signature reminder scheduled for signer {SignerId} at {ScheduledTime}",
                    signerId, scheduledTime
                );
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule signature reminder for signer {SignerId}", signerId);
                throw;
            }
        }
    }
}