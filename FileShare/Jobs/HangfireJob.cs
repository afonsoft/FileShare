using FileShare.Interfaces;
using FileShare.Repository;
using FileShare.Repository.Model;
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
            RecurringJob.AddOrUpdate<IHangfireJob>("DelOldFiles", x => x.JobDeleteOldFiles(null), Cron.Hourly(), TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate<IHangfireJob>("DelFilesNotExist", x => x.JobDeleteFilesNotExist(null), Cron.Hourly(), TimeZoneInfo.Local);
            RecurringJob.AddOrUpdate<IHangfireJob>("PermittedExtensions", x => x.JobImportPermittedExtensions(null), Cron.Hourly(), TimeZoneInfo.Local);
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

                var filesInDb = await _context.Files.Select(x => x.StorageName)
                                            .ToListAsync();

                var filesInDrive = Directory.GetFiles(_targetFilePath)
                                            .Select(x => x.Split("\\").Last())
                                            .ToList();

                var filesNotExistInDb = filesInDb.Where(x => !filesInDrive.Contains(x)).ToList();
                var filesNotExistInDrive = filesInDrive.Where(x => !filesInDb.Contains(x)).ToList();

                context.WriteLine($"Total de registros a deletar {filesNotExistInDb.Count}");
                context.WriteLine($"Total de arquivos a deletar {filesNotExistInDrive.Count}");

                foreach(var file in filesNotExistInDrive)
                {
                    try
                    {
                        var fileToDelete = Path.Combine(_targetFilePath, file);
                        if (File.Exists(fileToDelete) && !fileToDelete.Contains("index.htm") && !fileToDelete.Contains("Extensions.txt"))
                        {
                            context.WriteLine($"Delete file {fileToDelete}");
                            //File.Delete(fileToDelete);
                        }
                    }
                    catch (Exception ex)
                    {
                        context.WriteLine($"Error Delete file {file} : {ex}");
                    }
                }

                foreach (var file in filesNotExistInDb)
                {
                    try
                    {
                        var removeFile = await _context.Files
                                            .Where(x => x.StorageName == file)
                                            .FirstOrDefaultAsync();

                        if(removeFile != null)
                        {
                            context.WriteLine($"Remove file from DB {file}");
                            //_context.Files.Remove(removeFile);
                            //await _context.SaveChangesAsync();
                        }

                    }
                    catch (Exception ex)
                    {
                        context.WriteLine($"Error Remove file {file} from DB : {ex}");
                    }
                }

                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Error on Job JobDeleteFilesNotExist {context.BackgroundJob.Id}-{ex}");
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
                            context.WriteLine($"Error Delete file {file.StorageName} : {ex}");
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

        public async void JobImportPermittedExtensions(PerformContext context)
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
                int totalSucess = 0;
                int totalSkip = 0;
                int totalError = 0;
                int totalGeral = 0;

                var fileExtensions = Path.Combine(_targetFilePath, "Extensions.txt");
                if (File.Exists(fileExtensions))
                {
                    string[] lines = File.ReadAllLines(fileExtensions);

                  var progressBar = context.WriteProgressBar("Processo", 0);

                    foreach (string line in lines)
                    {
                        try
                        {
                            totalGeral++;
                            string ext = line.Split(';')[0];

                            if (!ext.StartsWith("."))
                                ext = "." + ext;

                            ext = ext.Trim().ToLower();

                           if (! _context.PermittedExtension.Any(x=> x.Extension == ext) && !string.IsNullOrEmpty(ext))
                            {
                               await _context.PermittedExtension.AddAsync(new ExtensionPermittedModel()
                                {
                                    CreationDateTime = DateTime.Now,
                                    Id = Guid.NewGuid(),
                                    Extension = ext
                                });
                                await _context.SaveChangesAsync();
                                totalSucess++;
                            }
                            else
                            {
                                totalSkip++;
                            }
                        }
                        catch(Exception ex)
                        {
                            totalError++;
                            context.WriteLine($"Erro na linha {line} : {ex}");
                            _logger.LogError(ex, $"Erro on Job JobImportPermittedExtensions {context.BackgroundJob.Id}-{line}-{ex}");
                        }

                        progressBar.SetValue((totalGeral / lines.Count()) * 100);
                    }

                    context.WriteLine($"Reultado: Sucessos ({totalSucess}) Ignorados ({totalSkip}) Falhas ({totalError})");
                }
                else
                {
                    context.WriteLine("Não tem o arquivo com as extensões para processar");
                }


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