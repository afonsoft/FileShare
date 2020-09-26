using FileShare.Filters;
using FileShare.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileShare.Controllers
{
    [GenerateAntiforgeryTokenCookie]
    public class DownloadController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DownloadController> _logger;
        private readonly string _targetFilePath;

        public DownloadController(ILogger<DownloadController> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
        }


        [HttpGet("/download")]
        public IActionResult Index()
        {
            return RedirectToAction("index", "home");
        }

        [HttpGet("/download/{hashCode}")]
        public async Task<IActionResult> Index(string hashCode)
        {
            if (string.IsNullOrEmpty(hashCode))
                return RedirectToAction("index", "home");

            var file = await _context.Files.FirstOrDefaultAsync(x => x.Hash == hashCode.ToUpper().Trim());

            if(file == null)
                return RedirectToAction("index", "home");

            var model = new FileShare.Models.FileModel
            {
                Hash = hashCode,
                Id = file.Id,
                Path = Path.Combine(_targetFilePath, file.StorageName),
                UntrustedName = file.StorageName,
                Size = file.Size,
                TrustedName = file.Name,
                Type = file.Type,
                UploadDT = file.CreationDateTime
            };

            return View(model);
        }
    }
}
