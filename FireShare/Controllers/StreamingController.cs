using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using FireShare.Attributes;
using FireShare.Utilities;
using FireShare.Repository;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using System;
using FireShare.Repository.Model;

namespace FireShare.Controllers
{
    [GenerateAntiforgeryTokenCookie]
    [DisableFormValueModelBinding]
    public class StreamingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _accessor;
        private readonly long _fileSizeLimit;
        private readonly ILogger<StreamingController> _logger;
        private readonly string[] _permittedExtensions = { ".txt", ".zip" };
        private readonly string _targetFilePath;

        // Get the default form options so that we can use them to set the default 
        // limits for request body data.
        private static readonly FormOptions _defaultFormOptions = new FormOptions();

        public StreamingController(ILogger<StreamingController> logger,
            ApplicationDbContext context, IWebHostEnvironment env,
            IHttpContextAccessor accessor)
        {
            _logger = logger;
            _context = context;
            _fileSizeLimit = int.MaxValue;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
            _accessor = accessor;
        }


        #region snippet_UploadPhysical
        [HttpPost]
        [DisableFormValueModelBinding]
        public async Task<IActionResult> UploadPhysical()
        {
            long contentType = 0;
            string fileNameForDisplay = "";
            string fileNameForFileStorage = "";

            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                ModelState.AddModelError("File",
                    $"The request couldn't be processed (Error 1).");
                // Log error

                return BadRequest(ModelState);
            }

            var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                _defaultFormOptions.MultipartBoundaryLengthLimit);

            var reader = new MultipartReader(boundary, HttpContext.Request.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                var hasContentDispositionHeader =
                    ContentDispositionHeaderValue.TryParse(
                        section.ContentDisposition, out var contentDisposition);

                if (hasContentDispositionHeader)
                {
                    // This check assumes that there's a file
                    // present without form data. If form data
                    // is present, this method immediately fails
                    // and returns the model error.
                    if (!MultipartRequestHelper
                        .HasFileContentDisposition(contentDisposition))
                    {
                        ModelState.AddModelError("File",
                            $"The request couldn't be processed (Error 2).");
                        // Log error

                        return BadRequest(ModelState);
                    }
                    else
                    {
                        // Don't trust the file name sent by the client. To display
                        // the file name, HTML-encode the value.
                        var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                                contentDisposition.FileName.Value);
                        fileNameForDisplay = trustedFileNameForDisplay;

                        var trustedFileNameForFileStorage = Path.GetRandomFileName();
                        fileNameForFileStorage = trustedFileNameForFileStorage;

                        var streamedFileContent = await FileHelpers.ProcessStreamedFile(
                            section, contentDisposition, ModelState,
                            _permittedExtensions, _fileSizeLimit);

                        if (!ModelState.IsValid)
                        {
                            return BadRequest(ModelState);
                        }

                        using (var targetStream = System.IO.File.Create(
                            Path.Combine(_targetFilePath, trustedFileNameForFileStorage)))
                        {
                            contentType += streamedFileContent.Count();
                            await targetStream.WriteAsync(streamedFileContent);

                            _logger.LogInformation(
                                "Uploaded file '{TrustedFileNameForDisplay}' saved to " +
                                "'{TargetFilePath}' as {TrustedFileNameForFileStorage}",
                                trustedFileNameForDisplay, _targetFilePath,
                                trustedFileNameForFileStorage);
                        }
                    }
                }

                // Drain any remaining section body that hasn't been consumed and
                // read the headers for the next section.
                section = await reader.ReadNextSectionAsync();
            }

            //Salvar os dados do arquivo no banco de dados.
            string hash = SaveInDb(HttpContext.Request.HttpContext.Connection.RemoteIpAddress.ToString(), fileNameForDisplay, fileNameForFileStorage, contentType);

            return Created(nameof(StreamingController), hash);
        }
        #endregion

        private string SaveInDb(string remoteIpAddress, string fileNameForDisplay, string fileNameForFileStorage, long contentType)
        {

            var fileUploaded = new FileModel
            {
                CreationDateTime = DateTime.Now,
                Id = Guid.NewGuid(),
                Size = contentType,
                IP = remoteIpAddress,
                Name = fileNameForDisplay,
                StorageName = fileNameForFileStorage,
                Type = Path.GetExtension(fileNameForDisplay).ToLowerInvariant(),
                Hash = CreateHashFile(fileNameForDisplay, fileNameForFileStorage, remoteIpAddress, contentType)
            };

            return fileUploaded.ToString();
        }

        private string CreateHashFile(string fileNameForDisplay, string fileNameForFileStorage, string remoteIp, long contentType)
        {
            return EncryptorHelpers.MD5Hash($"{fileNameForDisplay}|{fileNameForFileStorage}|{remoteIp}|{contentType}|{Guid.NewGuid()}");
        }
    }

    public class FormData
    {
        public string Note { get; set; }
    }
}
