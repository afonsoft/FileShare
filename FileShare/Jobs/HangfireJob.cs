using FileShare.Interfaces;
using FileShare.Repository;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;

namespace FileShare.Jobs
{
    public class HangfireJob : IHangfireJob
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HangfireJob> _logger;
        private readonly string _targetFilePath;
        public HangfireJob(ILogger<HangfireJob> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
        }
        public void Initialize()
        {
            RecurringJob.AddOrUpdate<IHangfireJob>("Delete OLD Files", x => x.JobDeleteOldFiles(null), Cron.Hourly, TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate<IHangfireJob>("Delete Files Not Exist", x => x.JobDeleteFilesNotExist(null), Cron.Daily, TimeZoneInfo.Local);
        }

        public async void JobDeleteFilesNotExist(PerformContext context)
        {
            try
            {
                context.WriteLine("Job Inicializado");
                
                var filesInDb = await _context.Files.ToListAsync();
                var filesInDrive = Directory.EnumerateFiles(_targetFilePath).ToList();

                var fdb = filesInDb.Select(x=>x.Name).ToList();

                var filesNotExistInDb =  "";
                var filesNotExistInDrive = "";


                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Erro on Job JobDeleteFilesNotExist {context.BackgroundJob.Id}-{ex}");
            }
        }

        public async void JobDeleteOldFiles(PerformContext context)
        {
            try
            {
                context.WriteLine("Job Inicializado");
                
                var filesInDb = await _context.Files
                                            .Where(x => x.CreationDateTime <= DateTime.Now.AddMonths(2))
                                            .ToListAsync();

                if (filesInDb.Any())
                {
                    context.WriteLine($"Total de arquivos a deletar {filesInDb.Count}");

                    foreach (var file in filesInDb)
                    {
                        try
                        {
                            var fileToDelete = Path.Combine(_targetFilePath, file.StorageName);
                            if (File.Exists(fileToDelete))
                            {
                                File.Delete(fileToDelete);
                            }
                            _context.Files.Remove(file);
                            await _context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            context.WriteLine($"Erro Delete file {file.StorageName} : {ex}");
                        }
                    }
                }
                else
                {
                    context.WriteLine("Nenhum arquivo para excluir");
                }

                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Erro on Job JobDeleteOldFiles {context.BackgroundJob.Id}-{ex}");
            }
        }
    }
}