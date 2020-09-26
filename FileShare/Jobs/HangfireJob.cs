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
        private static bool isInProcessJobDeleteFilesNotExist = false;
        private static bool isInProcessJobDeleteOldFiles = false;
        private static bool isInProcessJobImportPermittedExtensions = false;
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
            RecurringJob.AddOrUpdate<IHangfireJob>("Import Permitted Extensions", x => x.JobImportPermittedExtensions(null), Cron.Daily, TimeZoneInfo.Local);
        }

        public async void JobDeleteFilesNotExist(PerformContext context)
        {
            if (isInProcessJobDeleteFilesNotExist)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            isInProcessJobDeleteFilesNotExist = true;

            try
            {
                context.WriteLine("Job Inicializado");

                var filesInDb = await _context.Files.ToListAsync();
                var filesInDrive = Directory.GetFiles(_targetFilePath);

                var fdb = filesInDb.Select(x => x.Name).ToList();

                var filesNotExistInDb = "";
                var filesNotExistInDrive = "";


                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Erro on Job JobDeleteFilesNotExist {context.BackgroundJob.Id}-{ex}");
            }

            isInProcessJobDeleteFilesNotExist = false;
        }

        public async void JobDeleteOldFiles(PerformContext context)
        {
            if (isInProcessJobDeleteOldFiles)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            isInProcessJobDeleteOldFiles = true;

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

            isInProcessJobDeleteOldFiles = false;
        }

        public void JobImportPermittedExtensions(PerformContext context)
        {
            if (isInProcessJobImportPermittedExtensions)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            isInProcessJobImportPermittedExtensions = true;

            try
            {
                context.WriteLine("Job Inicializado");
                //Ler um arquivo txt com as extensões permitidas e salvar na base
               


                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Erro on Job JobImportPermittedExtensions {context.BackgroundJob.Id}-{ex}");
            }

            isInProcessJobImportPermittedExtensions = false;
        }
    }
}