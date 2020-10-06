using FileShare.Filters;
using Hangfire.Server;

namespace FileShare.Interfaces
{
    public interface IHangfireJob 
    {
        void Initialize();

        [DisableMultipleQueuedItemsFilter]
        void JobDeleteOldFiles(PerformContext context);
        
        [DisableMultipleQueuedItemsFilter]
        void JobDeleteFilesNotExist(PerformContext context);
        
        [DisableMultipleQueuedItemsFilter]
        void JobImportPermittedExtensions(PerformContext context);
    }
}
