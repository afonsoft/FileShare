using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using FileShare.Attributes;
using FileShare.Utilities;
using FileShare.Repository;
using Microsoft.AspNetCore.Hosting;
using System;
using FileShare.Repository.Model;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using FileShare.Net;

namespace FileShare.Controllers
{
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly long _fileSizeLimit;
        private readonly ILogger<StreamingController> _logger;
        private readonly string _targetFilePath;
        private const int BufferSize = 8192;
        private const int kbps = 100 * 1024; //100kbps

        #region PermittedExtensions
        private readonly string[] _permittedExtensions =
        {
            ".txt", ".zip", ".mpg", ".mp4", ".mp3", ".pdf", ".doc", ".docx", ".aac", ".abw",".arc",".avi",".azw",".bin",".bz",".bz2",".csh"  ,".css"  ,".csv"  ,".doc"  ,".eot"  ,".epub" ,".gif"  ,".htm"  ,".html" ,".ico"  ,".ics"  ,".jar"  ,".jpeg" ,".jpg"  ,".js"   ,".json" ,".mid"  ,".midi" ,".mpeg" ,".mpkg" ,".odp"  ,".ods"  ,".odt"  ,".oga"  ,".ogv"  ,".ogx"  ,".otf"  ,".png"  ,".pdf"  ,".ppt"  ,".rar"  ,".rtf"  ,".sh"   ,".svg"  ,".swf"  ,".tar"  ,".tif"  ,".tiff" ,".ts"   ,".ttf"  ,".vsd"  ,".wav"  ,".weba" ,".webm" ,".webp" ,".woff" ,".woff2",".xhtml",".xls"  ,".xlsx" ,".xml"  ,".xul"  ,".zip"  ,".3gp"  ,".3g2"  ,".7z"
        };
        #endregion

        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public StreamingController(ILogger<StreamingController> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _fileSizeLimit = int.MaxValue;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
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
                    ModelState.AddModelError("Error", $"The request couldn't be processed (Error 0).");
                    return BadRequest(ModelState);
                }

                var formAccumulator = new KeyValueAccumulator();
                var trustedFileNameForDisplay = string.Empty;
                var streamedFileContent = new byte[0];

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


                string trustedFileNameForFileStorage = Path.GetRandomFileName();
                fileNameFinaliy = Path.Combine(_targetFilePath, trustedFileNameForFileStorage);
                using (var targetStream = System.IO.File.Create(fileNameFinaliy))
                {
                    await targetStream.WriteAsync(streamedFileContent);
                    _logger.LogInformation($"Uploaded file '{trustedFileNameForDisplay}' saved to '{_targetFilePath}' as {trustedFileNameForFileStorage}");
                }

                var model = await SaveInDb(HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString(), fileNameFinaliy, trustedFileNameForDisplay, trustedFileNameForFileStorage, streamedFileContent.Length);

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
        #endregion

        #region DownloadFileStream
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult DownloadFileStream(Models.FileModel fileModel)
        {
            if (System.IO.File.Exists(fileModel.Path))
            {
                using (FileStream sourceStream = new FileStream(fileModel.Path, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    ThrottledStream destinationStream = new ThrottledStream(sourceStream, kbps);
                    return new FileStreamResult(destinationStream, FindMimeHelpers.GetMimeFromFile(fileModel.Path))
                    {
                        FileDownloadName = fileModel.TrustedName,
                        LastModified = fileModel.UploadDT
                    };
                }
            }
            else
            {
                ModelState.AddModelError("Error", $"The request couldn't be processed (Error 0).");
                return NotFound(ModelState);
            }
        }
        #endregion

        #region Private
        private async Task<Models.FileModel> SaveInDb(string remoteIpAddress,string fileNameFinaliy, string fileNameForDisplay, string fileNameForFileStorage, long contentType)
        {

            var fileUploaded = new FileModel
            {
                CreationDateTime = DateTime.Now,
                Id = Guid.NewGuid(),
                Size = contentType,
                IP = remoteIpAddress,
                Name = fileNameForDisplay,
                StorageName = fileNameForFileStorage,
                Type = FindMimeHelpers.GetMimeFromFile(fileNameFinaliy),
                Hash = CreateHashFile(fileNameForDisplay, fileNameForFileStorage, remoteIpAddress, contentType)
            };

            //Salvar no Banco de Dados
            await _context.Files.AddAsync(fileUploaded);
            await _context.SaveChangesAsync();

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

        #endregion
    }

    public class FormData
    {
        public string Note { get; set; }
    }
}