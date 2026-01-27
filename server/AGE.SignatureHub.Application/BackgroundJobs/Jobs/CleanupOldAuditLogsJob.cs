using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Persistence;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Application.BackgroundJobs.Jobs
{
    public class CleanupOldAuditLogsJob
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CleanupOldAuditLogsJob> _logger;

        public CleanupOldAuditLogsJob(IUnitOfWork unitOfWork, ILogger<CleanupOldAuditLogsJob> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ExecuteAsync(int daysToKeep = 365)
        {
            try
            {
                _logger.LogInformation("Starting cleanup of audit logs older than {DaysToKeep} days.", daysToKeep);

                var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);
                var oldLogs = await _unitOfWork.AuditLogs.FindAsync(log => log.Timestamp < cutoffDate);

                foreach (var log in oldLogs)
                {
                    await _unitOfWork.AuditLogs.DeleteAsync(log);
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Cleanup completed. Deleted {Count} audit logs.", oldLogs.Count());
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "An error occurred while cleaning up old audit logs.");
                throw;
            }
        }
    }
}