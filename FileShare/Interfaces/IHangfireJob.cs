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
        void JobDeleteOldFiles(PerformContext context);
             
        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        void JobDeleteFilesNotExist(PerformContext context);
        
        [DisableMultipleQueuedItemsFilter]
        [DisableConcurrentExecution(3600)]
        void JobImportPermittedExtensions(PerformContext context);
    }
}
