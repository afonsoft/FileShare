using Hangfire.Server;

namespace FireShare.Interfaces
{
    public interface IHangfireJob
    {
        void Initialize();
        void JobDeleteOldFiles(PerformContext context);
    }
}
