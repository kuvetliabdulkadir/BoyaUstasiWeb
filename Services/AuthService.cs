using Microsoft.AspNetCore.Identity;
using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Kimlik doğrulama işlemleri servisi (Identity)
    public class AuthService : IAuthService
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AuthService> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<(bool Succeeded, string? ErrorMessage, bool IsLockedOut)> LoginAsync(string emailOrUsername, string password, bool rememberMe, string? ipAddress)
        {
            // 1. Önce Email ile bulmaya çalış
            var user = await _userManager.FindByEmailAsync(emailOrUsername);
            
            // 2. Bulamazsa Username ile bulmaya çalış
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(emailOrUsername);
            }

            if (user == null)
            {
                _logger.LogWarning("Kullanıcı bulunamadı: {EmailOrUsername}", emailOrUsername);
                return (false, "Kullanıcı adı veya şifre hatalı.", false);
            }

            // Kilitli mi kontrol et
            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("Kilitli hesap ile giriş denemesi: {Email}", user.Email);
                return (false, "Hesabınız kilitlenmiş. Lütfen daha sonra tekrar deneyin.", true);
            }

            // PasswordSignInAsync userName bekler
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                rememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Son giriş bilgilerini güncelle
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = ipAddress;
                user.FailedLoginAttempts = 0;
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Başarılı giriş: {Email}", user.Email);
                return (true, null, false);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("Hesap kilitlendi: {Email}", user.Email);
                return (false, "Çok fazla başarısız deneme. Hesabınız 30 dakika kilitlenmiştir.", true);
            }

            _logger.LogWarning("Şifre hatası: {Email}", user.Email);
            return (false, "Kullanıcı adı veya şifre hatalı.", false);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Kullanıcı çıkış yaptı.");
        }
    }
}
