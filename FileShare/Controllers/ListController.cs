using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using FileShare.Models;
using FileShare.Repository;
using FileShare.Filters;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using FileShare.Grid;

namespace FileShare.Controllers
{
    [GenerateAntiforgeryTokenCookie]
    public class ListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ListController> _logger;
        private readonly string _targetFilePath;

        public ListController(ILogger<ListController> logger, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _logger = logger;
            _context = context;
            _targetFilePath = Path.Combine(env.WebRootPath, "FILES");
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<GridPagedOutput<FileModel>> Files(GridPagedInput input)
        {
            if (string.IsNullOrEmpty(input.SearchPhrase))
                input.SearchPhrase = "";

            var filesInDb = await _context.Files.ToListAsync();

            var files = filesInDb.Where(x => x.Id.ToString() == input.Id
                                    || x.Name.Contains(input.SearchPhrase)
                                    || x.StorageName.Contains(input.SearchPhrase)
                                    || x.Type.Contains(input.SearchPhrase)
                                    || x.Hash.Contains(input.SearchPhrase))
                                    .Skip((input.Current - 1) * input.RowCount)
                                    .Take(input.RowCount)
                                    .ToList();

            var models = files.Select(x => new FileModel
            {
                Hash = x.Hash,
                Id = x.Id,
                Path = Path.Combine(_targetFilePath, x.StorageName),
                UntrustedName = x.StorageName,
                Size = x.Size,
                TrustedName = x.Name,
                Type = x.Type,
                UploadDT = x.CreationDateTime
            }).ToList();

            return new GridPagedOutput<FileModel>(models) { Current = input.Current, RowCount = input.RowCount, Total = filesInDb.Count };

        }
    }
}