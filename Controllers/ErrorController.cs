using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;

namespace boya_usta_web.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("hata/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            _logger.LogInformation("ErrorController çağrıldı - StatusCode: {StatusCode}", statusCode);
            
            switch (statusCode)
            {
                case 400:
                    ViewBag.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
                    return View("~/Views/Error/400.cshtml");
                case 403:
                    return View("~/Views/Error/403.cshtml");
                case 404:
                    return View("~/Views/Error/404.cshtml");
                case 405:
                    _logger.LogInformation("405 sayfası gösteriliyor");
                    return View("~/Views/Error/405.cshtml");
                case 500:
                    var errorId = GenerateErrorId();
                    _logger.LogError("500 Hatası - ID: {ErrorId}", errorId);
                    ViewBag.ErrorId = errorId;
                    return View("~/Views/Error/500.cshtml");
                default:
                    _logger.LogWarning("Bilinmeyen status code: {StatusCode}, Error view gösteriliyor", statusCode);
                    return View("Error", new ErrorViewModel
                    {
                        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                    });
            }
        }

        [Route("hata/500")]
        public IActionResult ServerError()
        {
            var errorId = GenerateErrorId();
            _logger.LogError("500 Hatası - ID: {ErrorId}", errorId);
            ViewBag.ErrorId = errorId;
            return View("~/Views/Error/500.cshtml");
        }

        private string GenerateErrorId()
        {
            return $"ERR-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}

