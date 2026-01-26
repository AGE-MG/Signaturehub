using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Application.BackgroundJobs
{
    public interface IBackgroundJobsService
    {
        void ScheduleSignatureReminder(Guid signerId, DateTime scheduledTime);
        void CheckeExpiredDocuments();
        void CleanupOldAuditLogs(int daysToKeep);
    }
}