using FileShare.Interfaces;
using FileShare.IpData;
using FileShare.Repository;
using FileShare.Repository.Model;
using FileShare.Utilities;
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
        private readonly ILogger<HangfireJob> _logger;
        private readonly string _targetFilePath;
        private readonly IServiceProvider _serviceProvider;

        private static bool isInProcessJobDeleteFilesNotExist { get; set; } = false;
        private static bool isInProcessJobDeleteOldFiles { get; set; } = false;
        private static bool isInProcessJobImportPermittedExtensions { get; set; } = false;
        private static bool isInProcessJobImportNoewFiles { get; set; } = false;

        private readonly string[] _permittedExtensions;

        public HangfireJob(ILogger<HangfireJob> logger, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
            _serviceProvider = serviceProvider;
            _permittedExtensions = _serviceProvider.GetService<ApplicationDbContext>().PermittedExtension.Select(x => x.Extension).ToArray();
        }

        public void Initialize()
        {
            RecurringJob.AddOrUpdate<IHangfireJob>("DelOldFiles", x => x.JobDeleteOldFiles(null), Cron.Daily, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IHangfireJob>("DelFilesNotExist", x => x.JobDeleteFilesNotExist(null), Cron.Daily, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IHangfireJob>("PermittedExtensions", x => x.JobImportPermittedExtensions(null), Cron.Daily, TimeZoneInfo.Utc);
            RecurringJob.AddOrUpdate<IHangfireJob>("ImportNewFiles", x => x.JobImportNewFiles(null), Cron.Daily, TimeZoneInfo.Utc);
        }

        public async void JobDeleteFilesNotExist(PerformContext context)
        {
            if (isInProcessJobDeleteFilesNotExist)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            try
            {
                isInProcessJobDeleteFilesNotExist = true;
                context.WriteLine("Job Inicializado");
                ApplicationDbContext _context = _serviceProvider.GetService<ApplicationDbContext>();

                var filesInDb = await _context.Files.Select(x => x.StorageName)
                                            .ToListAsync();

                var filesInDrive = Directory.GetFiles(_targetFilePath)
                                            .Where(x => !x.Contains("index.htm")
                                                     && !x.Contains("Extensions.txt")
                                                     && !_permittedExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
                                            .Select(x => x.Split("\\").Last())
                                            .ToList();

                var filesNotExistInDb = filesInDb.Where(x => !filesInDrive.Contains(x)).ToList();
                var filesNotExistInDrive = filesInDrive.Where(x => !filesInDb.Contains(x)).ToList();

                context.WriteLine($"Total de registros a deletar {filesNotExistInDb.Count}");
                context.WriteLine($"Total de arquivos a deletar {filesNotExistInDrive.Count}");

                foreach (var file in filesNotExistInDrive)
                {
                    try
                    {
                        var fileToDelete = Path.Combine(_targetFilePath, file);
                        if (File.Exists(fileToDelete))
                        {
                            if (!filesInDb.Any(x => x.Contains(file)))
                            {
                                context.WriteLine($"Delete file {file}");
                                File.Delete(fileToDelete);
                            }
                            else
                            {
                                context.WriteLine($"File {file} not deleted");
                            }
                        }
                        else
                        {
                            context.WriteLine($"File {file} not exist");
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

                        if (removeFile != null)
                        {
                            if (!File.Exists(Path.Combine(_targetFilePath, file)))
                            {
                                context.WriteLine($"Remove file {file} from DB");
                                _context.Files.Remove(removeFile);
                                await _context.SaveChangesAsync();
                            }
                            else
                            {
                                context.WriteLine($"Not removed file {file} from DB becouse the file exist in drive");
                            }
                        }
                        else
                        {
                            context.WriteLine($"file {file} from DB not exist");
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
            finally
            {
                isInProcessJobDeleteFilesNotExist = false;
            }
        }

        public async void JobDeleteOldFiles(PerformContext context)
        {
            if (isInProcessJobDeleteOldFiles)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            try
            {
                isInProcessJobDeleteOldFiles = true;
                context.WriteLine("Job Inicializado");
                ApplicationDbContext _context = _serviceProvider.GetService<ApplicationDbContext>();

                var filesInUsers = await _context.FilesUsers
                                                .Where(x => x.CreationDateTime <= DateTime.Now.AddYears(-2))
                                                .Select(x => x.File)
                                                .ToListAsync();

                Guid[] guids = await _context.FilesUsers.Select(x => x.FileId).ToArrayAsync();

                var filesInDb = await _context.Files
                                            .Where(x => x.CreationDateTime <= DateTime.Now.AddMonths(-6))
                                            .ToListAsync();

                if (filesInDb.Any() || filesInUsers.Any())
                {
                    context.WriteLine($"Total de arquivos a deletar 2 Meses {filesInDb.Count}");
                    context.WriteLine($"Total de arquivos de usuários a deletar 6 Meses {filesInUsers.Count}");

                    context.WriteLine($"Deletando os arquivos com mais 2 Meses");
                    foreach (var file in filesInDb)
                    {
                        try
                        {
                            if (!guids.Contains(file.Id))
                            {
                                var fileToDelete = Path.Combine(_targetFilePath, file.StorageName);
                                if (File.Exists(fileToDelete))
                                {
                                    File.Delete(fileToDelete);
                                }
                                _context.Files.Remove(file);
                                await _context.SaveChangesAsync();
                                context.WriteLine($"Delete/Remove file {file.StorageName}");
                            }
                            else
                            {
                                context.WriteLine($"File {file.StorageName} is the user");
                            }
                        }
                        catch (Exception ex)
                        {
                            context.WriteLine($"Error Delete file {file.StorageName} : {ex}");
                        }
                    }

                    context.WriteLine($"Deletando os arquivos com mais 12 Meses");
                    foreach (var file in filesInUsers)
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
                            context.WriteLine($"Delete/Remove file {file.StorageName}");
                        }
                        catch (Exception ex)
                        {
                            context.WriteLine($"Error Delete file {file.StorageName} : {ex}");
                        }
                    }
                }
                else
                {
                    context.WriteLine("Nenhum arquivo para remover do DB");
                }

                context.WriteLine("Job Finalizado");
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Erro on Job JobDeleteOldFiles {context.BackgroundJob.Id}-{ex}");
            }
            finally
            {
                isInProcessJobDeleteOldFiles = false;
            }
        }

        public async void JobImportPermittedExtensions(PerformContext context)
        {
            if (isInProcessJobImportPermittedExtensions)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            try
            {
                isInProcessJobImportPermittedExtensions = true;
                context.WriteLine("Job Inicializado");
                ApplicationDbContext _context = _serviceProvider.GetService<ApplicationDbContext>();
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
                            string[] l = line.Split(';');
                            string ext = l[0];
                            if (!ext.StartsWith("."))
                                ext = "." + ext;

                            ext = ext.Trim().ToLower();
                            string description = "application/octet-stream";
                            if (l.Length > 1 && !string.IsNullOrEmpty(l[1]))
                                description = l[1].ToLower().Trim();

                            if (!string.IsNullOrEmpty(ext))
                            {
                                var model = await _context.PermittedExtension.FirstOrDefaultAsync(x => x.Extension == ext);

                                if (model == null)
                                {
                                    await _context.PermittedExtension.AddAsync(new ExtensionPermittedModel()
                                    {
                                        CreationDateTime = DateTime.UtcNow,
                                        Id = Guid.NewGuid(),
                                        Extension = ext,
                                        Description = description
                                    });
                                    await _context.SaveChangesAsync();
                                    totalSucess++;
                                }
                                else
                                {
                                    if (model.Description != description && !string.IsNullOrEmpty(description))
                                    {
                                        model.Description = description;
                                        model.Extension = ext;
                                        _context.PermittedExtension.Update(model);
                                        await _context.SaveChangesAsync();
                                        totalSucess++;
                                    }
                                    else
                                    {
                                        totalSkip++;
                                    }
                                }
                            }
                            else
                            {
                                totalSkip++;
                            }
                        }
                        catch (Exception ex)
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
            finally
            {
                isInProcessJobImportPermittedExtensions = false;
            }
        }

        public async void JobImportNewFiles(PerformContext context)
        {
            if (isInProcessJobImportNoewFiles)
            {
                context.WriteLine("Job já em processamento.");
                return;
            }

            try
            {
                isInProcessJobImportNoewFiles = true;
                context.WriteLine("Job Inicializado");

                ApplicationDbContext _context = _serviceProvider.GetService<ApplicationDbContext>();

                var filesInDrive = Directory.GetFiles(_targetFilePath)
                                             .Where(x => !x.Contains("index.htm")
                                                      && !x.Contains("Extensions.txt")
                                                      && _permittedExtensions.Contains(Path.GetExtension(x).ToLowerInvariant()))
                                             .Select(x => x.Split("\\").Last())
                                             .ToList();

                var filesInDb = await _context.Files.Select(x => x.StorageName)
                                            .ToListAsync();

                var filesNotInported = filesInDrive.Where(x => !filesInDb.Contains(x)).ToList();

                context.WriteLine($"Total de arquivos a importar {filesNotInported.Count}");
                var ipInfo = await new IpDataServlet().GetIpInfo();

                foreach (var file in filesNotInported)
                {
                    try
                    {
                        var fileWithPath = Path.Combine(_targetFilePath, file);

                        if (File.Exists(fileWithPath))
                        {
                            FileInfo info = new FileInfo(fileWithPath);
                            var contentType = FindMimeHelpers.GetMimeFromFile(fileWithPath);
                            FileModel model = new FileModel
                            {
                                CreationDateTime = DateTime.UtcNow,
                                Id = Guid.NewGuid(),
                                Size = info.Length,
                                Name = file,
                                StorageName = file,
                                Type = contentType,
                                Hash = CreateHashFile(file, file, ipInfo.Ip, info.Length),
                                Asn = ipInfo.Asn,
                                AsnDomain = ipInfo.AsnDomain,
                                AsnName = ipInfo.AsnName,
                                AsnRoute = ipInfo.AsnRoute,
                                AsnType = ipInfo.AsnType,
                                CallingCode = ipInfo.CallingCode,
                                City = ipInfo.City,
                                ContinentCode = ipInfo.ContinentCode,
                                ContinentName = ipInfo.ContinentName,
                                CountryCode = ipInfo.CountryCode,
                                CountryName = ipInfo.CountryName,
                                Ip = ipInfo.Ip,
                                Latitude = ipInfo.Latitude,
                                Longitude = ipInfo.Longitude,
                                Organisation = ipInfo.Organisation,
                                Postal = ipInfo.Postal,
                                Region = ipInfo.Region,
                                RegionCode = ipInfo.RegionCode,
                                TimeZone = ipInfo.TimeZone,
                                Languages = ipInfo.Languages
                            };

                            _context.Files.Add(model);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            context.WriteLine($"Arquivo invalido {fileWithPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        context.WriteLine($"Erro no Arquivo {file} : {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                context.WriteLine($"Erro no Job : {ex}");
                _logger.LogError(ex, $"Error on Job JobImportNewFiles {context.BackgroundJob.Id}-{ex}");
            }
            finally
            {
                isInProcessJobImportNoewFiles = false;
            }
        }

        private string CreateHashFile(string fileNameForDisplay, string fileNameForFileStorage, string remoteIp, long contentType)
        {
            return EncryptorHelpers.MD5Hash($"{fileNameForDisplay}|{fileNameForFileStorage}|{remoteIp}|{contentType}|{Guid.NewGuid()}");
        }
    }
}