using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
 
    // Admin paneli için kullanıcı oturum işlemlerini (Giriş, Çıkış) yöneten kontrolcü.
    [Route("usta-panel-2024")]
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        // Servislerin enjekte edilmesi
        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // Giriş sayfası görüntüleme
        [HttpGet("giris")]
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Login(string? returnUrl = null)
        {
            // Kullanıcı zaten giriş yapmışsa dashboard'a yönlendir
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Giriş işlemi (POST)
        [HttpPost("giris")]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var (succeeded, errorMessage, isLockedOut) = await _authService.LoginAsync(model.Email, model.Password, model.RememberMe, ipAddress);

            if (succeeded)
            {
                return RedirectToLocal(returnUrl);
            }

            if (isLockedOut)
            {
                ModelState.AddModelError(string.Empty, errorMessage ?? "Hesabınız kilitlendi.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, errorMessage ?? "Giriş başarısız.");
            return View(model);
        }

        // Çıkış işlemi (POST)
        [HttpPost("cikis")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login");
        }

        // Erişim engellendi sayfası
        [HttpGet("erisim-engellendi")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Yerel URL'e yönlendirme yardımı
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

