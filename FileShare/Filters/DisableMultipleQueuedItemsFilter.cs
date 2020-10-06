using Hangfire.Client;
using Hangfire.Common;
using Hangfire.Logging;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;

namespace FileShare.Filters
{
    public class DisableMultipleQueuedItemsFilterAttribute : JobFilterAttribute, IClientFilter, IServerFilter, IApplyStateFilter
    {
        private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan FingerprintTimeout = TimeSpan.FromHours(1);
        private static readonly ILog Logger = LogProvider.GetCurrentClassLogger();

        public void OnCreating(CreatingContext filterContext)
        {
            Logger.InfoFormat("Creating a job based on method `{0}`...", filterContext.Job.Method.Name);
            if (!AddFingerprintIfNotExists(filterContext.Connection, filterContext.Job))
            {
                filterContext.Canceled = true;
            }
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            Logger.InfoFormat("Starting to perform job `{0}`", filterContext.BackgroundJob.Id);
            if (filterContext.Exception == null || filterContext.ExceptionHandled)
            {
                RemoveFingerprint(filterContext.Connection, filterContext.BackgroundJob.Job);
            }
        }

        private static bool AddFingerprintIfNotExists(IStorageConnection connection, Job job)
        {
            using (connection.AcquireDistributedLock(GetFingerprintLockKey(job), LockTimeout))
            {
                var fingerprint = connection.GetAllEntriesFromHash(GetFingerprintKey(job));

                DateTimeOffset timestamp;

                if (fingerprint != null &&
                    fingerprint.ContainsKey("Timestamp") &&
                    DateTimeOffset.TryParse(fingerprint["Timestamp"], null, DateTimeStyles.RoundtripKind, out timestamp) &&
                    DateTimeOffset.UtcNow <= timestamp.Add(FingerprintTimeout))
                {
                    // Actual fingerprint found, returning.
                    return false;
                }

                // Fingerprint does not exist, it is invalid (no `Timestamp` key),
                // or it is not actual (timeout expired).
                connection.SetRangeInHash(GetFingerprintKey(job), new Dictionary<string, string>
                {
                    { "Timestamp", DateTimeOffset.UtcNow.ToString("o") }
                });

                return true;
            }
        }

        private static void RemoveFingerprint(IStorageConnection connection, Job job)
        {
            using (connection.AcquireDistributedLock(GetFingerprintLockKey(job), LockTimeout))
            using (var transaction = connection.CreateWriteTransaction())
            {
                transaction.RemoveHash(GetFingerprintKey(job));
                transaction.Commit();
            }
        }

        private static string GetFingerprintLockKey(Job job)
        {
            return String.Format("{0}:lock", GetFingerprintKey(job));
        }

        private static string GetFingerprintKey(Job job)
        {
            return String.Format("fingerprint:{0}", GetFingerprint(job));
        }

        private static string GetFingerprint(Job job)
        {
            var parameters = string.Empty;
            if (job?.Args != null)
            {
                parameters = string.Join(".", job.Args);
            }
            if (job?.Type == null || job.Method == null)
            {
                return string.Empty;
            }
            var payload = $"{job.Type.FullName}.{job.Method.Name}.{parameters}";
            var hash = SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
            var fingerprint = Convert.ToBase64String(hash);
            return fingerprint;
        }

        public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            try
            {
                Logger.InfoFormat(
                "Job `{0}` state was changed from `{1}` to `{2}`",
                context.BackgroundJob.Id,
                context.OldStateName,
                context.NewState.Name);

                context.JobExpirationTimeout = TimeSpan.FromDays(12);

                var failedState = context.NewState is FailedState;
                var deletedState = context.NewState is DeletedState;
                if (failedState || deletedState)
                {
                    RemoveFingerprint(context.Connection, context.BackgroundJob.Job);
                }
            }
            catch (Exception)
            {
                // Unhandled exceptions can cause an endless loop.
                // Therefore, catch and ignore them all.
            }
        }

        public void OnCreated(CreatedContext filterContext)
        {
            Logger.InfoFormat(
            "Job that is based on method `{0}` has been created with id `{1}`",
            filterContext.Job.Method.Name,
            filterContext.BackgroundJob?.Id);
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            Logger.InfoFormat("Starting to perform job `{0}`", filterContext.BackgroundJob.Id);
        }

        public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
        {
            Logger.InfoFormat(
             "Job `{0}` state `{1}` was unapplied.",
             context.BackgroundJob.Id,
             context.OldStateName);

            context.JobExpirationTimeout = TimeSpan.FromDays(12);
        }
    }
}