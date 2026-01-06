using Microsoft.AspNetCore.Identity;

namespace boya_usta_web.Models
{
    // Uygulama kullanıcısı sınıfı (IdentityUser'dan türetilmiştir)
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public string? LastLoginIp { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;

        public DateTime? LockoutEndDate { get; set; }
    }
}

