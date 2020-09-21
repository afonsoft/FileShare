using FireShare.Interfaces;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using System;

namespace FireShare.Jobs
{
    public class HangfireJob : IHangfireJob
    {
        public void Initialize()
        {
            RecurringJob.AddOrUpdate<IHangfireJob>("Delete OLD Files", x => x.JobDeleteOldFiles(null), Cron.Hourly, TimeZoneInfo.Local);
          }

        public void JobDeleteOldFiles(PerformContext context)
        {
            try
            {
                context.WriteLine("Job Inicializado");
                //Do
                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
            }
        }
    }
}
