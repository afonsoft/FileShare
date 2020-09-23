using FireShare.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace FireShare.Controllers
{
    [GenerateAntiforgeryTokenCookie]
    public class DownloadController : Controller
    {
        [HttpGet("/download")]
        public IActionResult Index()
        {
            return RedirectToAction("index", "home");
        }

        [HttpGet("/download/{hashCode}")]
        public IActionResult Index(string hashCode)
        {
            if (string.IsNullOrEmpty(hashCode))
                return RedirectToAction("index", "home");

            return View(new FireShare.Models.FileModel { Id = System.Guid.NewGuid(), Hash = "ASDASDASDASD", Size = 12313123123, UntrustedName = "asdasd.ass", TrustedName="arquivo.zip", Path="c:\\", UploadDT = System.DateTime.Now });
        }
    }
}
