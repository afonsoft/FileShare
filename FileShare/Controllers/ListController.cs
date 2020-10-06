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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace FileShare.Controllers
{
    [GenerateAntiforgeryToken]
    [Authorize]
    public class ListController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ListController> _logger;
        private readonly string _targetFilePath;
        private readonly UserManager<ApplicationIdentityUser> _userManager;

        public ListController(ILogger<ListController> logger, ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationIdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
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

            if (HttpContext.User != null)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user != null)
                {
                    bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    IList<Repository.Model.FileModel> filesInDb;
                    
                    if (isAdmin)
                    {
                        filesInDb = await _context.Files.ToListAsync();
                    }
                    else
                    {
                        filesInDb = await _context.FilesUsers
                                                        .Where(x => x.UserId == user.Id)
                                                        .Select(x => x.File)
                                                        .ToListAsync();
                    }

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
            return new GridPagedOutput<FileModel>(null) { Current = input.Current, RowCount = input.RowCount, Total = 0 };

        }
    }
}