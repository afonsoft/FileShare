using FileShare.Models;
using FileShare.Repository;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FileShare.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;
        private readonly ApplicationDbContext _context;

        public ErrorController(ILogger<ErrorController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/Error/{statusCode:int}")]
        public IActionResult Index(int? statusCode = null)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            if (statusCode.HasValue)
            {
                this.HttpContext.Response.StatusCode = statusCode.Value;
            }

            string originalPath = "";
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                originalPath = HttpContext.Items["originalPath"] as string;
            }

            if (string.IsNullOrEmpty(originalPath))
            {
                originalPath = exceptionHandlerPathFeature?.Path;
            }

            if (string.IsNullOrEmpty(originalPath))
            {
                originalPath = "unknown";
            }

            string message = exceptionHandlerPathFeature?.Error?.Message;

            if (string.IsNullOrEmpty(message))
            {
                switch (this.HttpContext.Response.StatusCode)
                {
                    case 404:
                        message = "Not Found";
                        break;
                    case 500:
                    case 501:
                    case 502:
                    case 503:
                    case 504:
                        message = "Internal Server Error / Service Unavailable / Bad Gateway";
                        break;
                    case 401:
                    case 403:
                    case 406:
                    case 407:
                        message = "Sorry, your access is refused due to security reasons of our server and also our sensitive data.";
                        break;
                }
            }

            return View(new ErrorModel
            {
                StatusCode = statusCode.ToString(),
                Message = message ?? "Unknown Error",
                Path = originalPath
            });
        }
    }
}