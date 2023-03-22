using FileShare.Memory;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileShare.Utilities
{
    public static class FileHelpers
    {
        public static async Task<HugeMemoryStream> ProcessStreamedFile(
            MultipartSection section, ContentDispositionHeaderValue contentDisposition,
            ModelStateDictionary modelState, string[] permittedExtensions, long sizeLimit)
        {
            try
            {
                var memoryStream = new HugeMemoryStream();

                await section.Body.CopyToAsync(memoryStream);

                // Check if the file is empty or exceeds the size limit.
                if (memoryStream.Length == 0)
                {
                    modelState.AddModelError("Error", "The file is empty.");
                }
                else if (memoryStream.Length > sizeLimit)
                {
                    var megabyteSizeLimit = sizeLimit / 1048576;
                    modelState.AddModelError("Error", $"The file exceeds {megabyteSizeLimit:N1} MB.");
                }
                else if (!IsValidFileExtensionAndSignature(contentDisposition.FileName.Value, memoryStream, permittedExtensions))
                {
                    modelState.AddModelError("Error", $"The file type isn't permitted or the file's signature doesn't match the file's extension. {Path.GetExtension(contentDisposition.FileName.Value).ToLowerInvariant()}");
                }
                else
                {
                    if (!IsValidFileExtensionAndSignature(memoryStream, permittedExtensions))
                    {
                        modelState.AddModelError("Error", $"The file type isn't permitted or the file's signature doesn't match the file's extension. {FindMimeHelpers.GetMimeFromStream(memoryStream)} - {FindMimeHelpers.GetExtensionsFromStream(memoryStream)[0]}");
                    }

                    return memoryStream;
                }
            }
            catch (Exception ex)
            {
                modelState.AddModelError("Error", $"The upload failed. Please contact the Help Desk for support. Error: {ex.Message}");
                modelState.AddModelError("Exception", ex.Message);
                modelState.AddModelError("StackTrace", ex.StackTrace);
            }

            return new HugeMemoryStream();
        }

        private static bool IsValidFileExtensionAndSignature(Stream stream, string[] permittedExtensions)
        {
            try
            {
                var ext = "." + FindMimeHelpers.GetExtensionsFromStream(stream)[0];

                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return true;
            }
        }

        private static bool IsValidFileExtensionAndSignature(string fileName, Stream data, string[] permittedExtensions)
        {
            if (string.IsNullOrEmpty(fileName) || data == null || data.Length == 0)
            {
                return false;
            }

            var ext = Path.GetExtension(fileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
            {
                return false;
            }

            data.Position = 0;

            return true;
        }
    }
}