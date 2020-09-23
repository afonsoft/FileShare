using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FireShare.Controllers
{
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

            return View();
        }
    }
}
