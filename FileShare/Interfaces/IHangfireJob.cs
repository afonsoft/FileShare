using FileShare.Filters;
using Hangfire;
using Hangfire.Server;

namespace FileShare.Interfaces
{
    public interface IHangfireJob 
    {
        void Initialize();

        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 0)]
        void JobDeleteOldFiles(PerformContext context);
             
        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 0)]
        void JobDeleteFilesNotExist(PerformContext context);
        
        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 0)]
        void JobImportPermittedExtensions(PerformContext context);

        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        [AutomaticRetry(Attempts = 0)]
        void JobImportNewFiles(PerformContext context);
    }
}
