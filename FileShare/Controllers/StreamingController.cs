﻿using FileShare.Attributes;
using FileShare.IpData;
using FileShare.Memory;
using FileShare.Net;
using FileShare.Repository;
using FileShare.Repository.Model;
using FileShare.Utilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileShare.Controllers
{
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly long _fileSizeLimit;
        private readonly ILogger<StreamingController> _logger;
        private readonly string _targetFilePath;
        private const int BufferSize = 2048;
        private const int kbps = 1024 * 1024; //1024kbps or 1Mbps
        private const int kbpsLogged = 1024 * 1024 * 4; //4096kbps or 4Mbps
        private readonly string[] _permittedExtensions;
        private readonly UserManager<ApplicationIdentityUser> _userManager;
        private readonly SignInManager<ApplicationIdentityUser> _signInManager;

        // Get the default form options so that we can use them to set the default
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public StreamingController(ILogger<StreamingController> logger, ApplicationDbContext context, IWebHostEnvironment env, SignInManager<ApplicationIdentityUser> signInManager, UserManager<ApplicationIdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _fileSizeLimit = int.MaxValue;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
            _permittedExtensions = context.PermittedExtension.Select(x => x.Extension).ToArray();
        }

        #region UploadFileStream

        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadFileStream()
        {
            string fileNameFinaliy = "";
            try
            {
                if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
                {
                    ModelState.AddModelError("Error", $"The request couldn't be processed (Error -1).");
                    return BadRequest(ModelState);
                }

                if (!MultipartRequestHelper.ValidateAntiforgeryToken(Request.Headers))
                {
                    ModelState.AddModelError("Error", $"The request couldn't be processed (Error 0).");
                    return BadRequest(ModelState);
                }

                var formAccumulator = new KeyValueAccumulator();
                var trustedFileNameForDisplay = string.Empty;
                var streamedFileContent = new HugeMemoryStream();

                var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType), _defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary, HttpContext.Request.Body);

                var section = await reader.ReadNextSectionAsync();

                if (section == null)
                {
                    ModelState.AddModelError("Error", $"The request couldn't be processed (Error 1).");
                    return BadRequest(ModelState);
                }

                while (section != null)
                {
                    var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition);

                    if (hasContentDispositionHeader)
                    {
                        if (MultipartRequestHelper.HasFileContentDisposition(contentDisposition))
                        {
                            trustedFileNameForDisplay = WebUtility.HtmlEncode(contentDisposition.FileName.Value);
                            streamedFileContent = await FileHelpers.ProcessStreamedFile(section, contentDisposition, ModelState, _permittedExtensions, _fileSizeLimit);

                            if (!ModelState.IsValid)
                            {
                                return BadRequest(ModelState);
                            }
                        }
                        else if (MultipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                        {
                            var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;
                            var encoding = GetEncoding(section);

                            if (encoding == null)
                            {
                                ModelState.AddModelError("Error", $"The request couldn't be processed (Error 2).");
                                return BadRequest(ModelState);
                            }

                            using (var streamReader = new StreamReader(
                                section.Body,
                                encoding,
                                detectEncodingFromByteOrderMarks: true,
                                bufferSize: 1024,
                                leaveOpen: true))
                            {
                                var value = await streamReader.ReadToEndAsync();

                                if (string.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                                {
                                    value = string.Empty;
                                }

                                formAccumulator.Append(key, value);

                                if (formAccumulator.ValueCount > _defaultFormOptions.ValueCountLimit)
                                {
                                    ModelState.AddModelError("Error", $"The request couldn't be processed (Error 3).");

                                    return BadRequest(ModelState);
                                }
                            }
                        }
                    }

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }

                    section = await reader.ReadNextSectionAsync();
                }

                // Bind form data to the model
                var formData = new FormData();
                var formValueProvider = new FormValueProvider(BindingSource.Form, new FormCollection(formAccumulator.GetResults()), CultureInfo.CurrentCulture);
                var bindingSuccessful = await TryUpdateModelAsync(formData, prefix: "", valueProvider: formValueProvider);

                if (!bindingSuccessful)
                {
                    ModelState.AddModelError("Error", "The request couldn't be processed (Error 5).");
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrEmpty(trustedFileNameForDisplay) || streamedFileContent.Length <= 0)
                {
                    ModelState.AddModelError("Error", "The request couldn't be processed (Error 6).");
                    return BadRequest(ModelState);
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                string trustedFileNameForFileStorage = Path.GetRandomFileName();
                fileNameFinaliy = Path.Combine(_targetFilePath, trustedFileNameForFileStorage);
                string mimeType = FindMimeHelpers.GetMimeFromStream(streamedFileContent);

                using (var targetStream = System.IO.File.Create(fileNameFinaliy))
                {
                    streamedFileContent.CopyTo(targetStream);
                    _logger.LogInformation($"Uploaded file '{trustedFileNameForDisplay}' saved to '{_targetFilePath}' as {trustedFileNameForFileStorage}");
                }

                var model = await SaveInDb(HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString(), fileNameFinaliy, trustedFileNameForDisplay, trustedFileNameForFileStorage, streamedFileContent.Length, mimeType);

                return new OkObjectResult(model.Hash.Trim().ToLower());
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(fileNameFinaliy) && System.IO.File.Exists(fileNameFinaliy))
                    System.IO.File.Delete(fileNameFinaliy);

                ModelState.AddModelError("Error", ex.Message);

                if (ex.InnerException != null)
                    ModelState.AddModelError("InnerException", ex.InnerException.Message);

                return BadRequest(ModelState);
            }
        }

        #endregion UploadFileStream

        #region DownloadFileStream

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> DownloadFileStream(Models.FileModel fileModel)
        {
            if (System.IO.File.Exists(fileModel.Path))
            {
                FileStream sourceStream = new FileStream(fileModel.Path, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);
                ThrottledStream destinationStreamStream = null;
                ApplicationIdentityUser user = null;
                if (HttpContext.User != null)
                {
                    user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        destinationStreamStream = new ThrottledStream(sourceStream, kbpsLogged);
                    }
                }

                if (destinationStreamStream == null)
                    destinationStreamStream = new ThrottledStream(sourceStream, kbps);

                await LoggerDownload(user, fileModel);

                FileStreamResult fileStreamResult = new FileStreamResult(destinationStreamStream, FindMimeHelpers.GetMimeFromFile(fileModel.Path));
                fileStreamResult.FileDownloadName = fileModel.TrustedName;
                fileStreamResult.LastModified = fileModel.UploadDT;

                return fileStreamResult;
            }
            else
            {
                ModelState.AddModelError("Error", $"The request couldn't be processed (Error 0).");
                return NotFound(ModelState);
            }
        }

        private async Task LoggerDownload(ApplicationIdentityUser user, Models.FileModel fileModel)
        {
            string ip = HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString();
            Guid? userId = user?.Id;
            var ipInfo = await new IpDataServlet(ip).GetIpInfo();

            LoggerDownloadModel logDown = new LoggerDownloadModel
            {
                CreationDateTime = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                UserId = userId,
                Size = fileModel.Size,
                Name = fileModel.TrustedName,
                StorageName = fileModel.UntrustedName,
                Type = fileModel.Type,
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
                Languages = ipInfo.Languages,
                Error = ipInfo.Error
            };

            await _context.LoggerDownload.AddAsync(logDown);
            await _context.SaveChangesAsync();
        }

        #endregion DownloadFileStream

        #region Private

        private async Task<Models.FileModel> SaveInDb(string remoteIpAddress, string fileNameFinaliy, string fileNameForDisplay, string fileNameForFileStorage, long contentType, string mimeType)
        {
            var ipInfo = await new IpDataServlet(remoteIpAddress).GetIpInfo();

            var fileUploaded = new FileModel
            {
                CreationDateTime = DateTime.UtcNow,
                Id = Guid.NewGuid(),
                Size = contentType,
                Name = fileNameForDisplay,
                StorageName = fileNameForFileStorage,
                Type = mimeType,
                Hash = CreateHashFile(fileNameForDisplay, fileNameForFileStorage, remoteIpAddress, contentType),

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

            try
            {
                if (mimeType == "application/octet-stream" || string.IsNullOrEmpty(mimeType))
                    mimeType = FindMimeHelpers.GetMimeFromFile(fileNameFinaliy);

                if (mimeType == "application/octet-stream" || string.IsNullOrEmpty(mimeType))
                {
                    if (FindMimeHelpers.ListOfMimeType == null || FindMimeHelpers.ListOfMimeType.Count <= 0)
                    {
                        FindMimeHelpers.ListOfMimeType = _context.PermittedExtension
                                                            .Select(x => new { x.Extension, x.Description })
                                                            .ToDictionary(x => x.Extension, x => x.Description);
                    }

                    fileUploaded.Type = FindMimeHelpers.GetMimeFromExtensions(Path.GetExtension(fileNameForDisplay).ToLowerInvariant());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro para recuperar o MineType : " + ex.Message);

                if (FindMimeHelpers.ListOfMimeType == null || FindMimeHelpers.ListOfMimeType.Count <= 0)
                {
                    FindMimeHelpers.ListOfMimeType = _context.PermittedExtension
                                                        .Select(x => new { x.Extension, x.Description })
                                                        .ToDictionary(x => x.Extension, x => x.Description);
                }

                fileUploaded.Type = FindMimeHelpers.GetMimeFromExtensions(Path.GetExtension(fileNameForDisplay).ToLowerInvariant());
            }

            if (string.IsNullOrEmpty(fileUploaded.Type))
                fileUploaded.Type = "application/octet-stream";

            //Salvar no Banco de Dados
            await _context.Files.AddAsync(fileUploaded);
            await _context.SaveChangesAsync();

            ApplicationIdentityUser user = null;
            try
            {
                if (HttpContext.User != null)
                {
                    user = await _userManager.GetUserAsync(HttpContext.User);
                    if (user != null)
                    {
                        await _context.FilesUsers.AddAsync(new FileUserModel
                        {
                            Id = Guid.NewGuid(),
                            FileId = fileUploaded.Id,
                            UserId = user.Id,
                            CreationDateTime = DateTime.Now
                        });
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro para associar o arquivo {fileUploaded.Id} ao usuário {user?.Id} - {ex}");
            }

            return new Models.FileModel
            {
                Hash = fileUploaded.Hash,
                Id = fileUploaded.Id,
                Size = fileUploaded.Size,
                UntrustedName = fileUploaded.Name,
                UploadDT = fileUploaded.CreationDateTime
            };
        }

        private string CreateHashFile(string fileNameForDisplay, string fileNameForFileStorage, string remoteIp, long contentType)
        {
            return EncryptorHelpers.MD5Hash($"{fileNameForDisplay}|{fileNameForFileStorage}|{remoteIp}|{contentType}|{Guid.NewGuid()}");
        }

        private static Encoding GetEncoding(MultipartSection section)
        {
            var hasMediaTypeHeader =
                MediaTypeHeaderValue.TryParse(section.ContentType, out var mediaType);

            // UTF-7 is insecure and shouldn't be honored. UTF-8 succeeds in
            // most cases.
            if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
            {
                return Encoding.UTF8;
            }

            return mediaType.Encoding;
        }

        #endregion Private
    }

    public class FormData
    {
        public string Note { get; set; }
    }
}