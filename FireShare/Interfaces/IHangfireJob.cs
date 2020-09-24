using Hangfire.Server;

namespace FileShare.Interfaces
{
    public interface IHangfireJob
    {
        void Initialize();
        void JobDeleteOldFiles(PerformContext context);
    }
}
