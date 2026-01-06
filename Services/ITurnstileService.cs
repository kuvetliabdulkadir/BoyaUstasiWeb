namespace boya_usta_web.Services
{
    // Cloudflare Turnstile CAPTCHA doğrulama servis arayüzü
    public interface ITurnstileService
    {
        // Cloudflare Turnstile veya reCAPTCHA token'ını doğrular
        // token: Form'dan gelen doğrulama token'ı
        // remoteIp: Kullanıcının IP adresi
        // returns: Doğrulama başarılı ise true
        Task<bool> ValidateTokenAsync(string token, string? remoteIp = null);
        
        // CAPTCHA'nın aktif olup olmadığını kontrol eder
        bool IsEnabled { get; }
        
        // Site key'i (frontend için)
        string SiteKey { get; }
        
        // CAPTCHA göstermek için gereken minimum form gönderim sayısı
        int ThresholdCount { get; }
        
        // Gönderim sayacının sıfırlanacağı süre (dakika)
        int ThresholdMinutes { get; }
        // CAPTCHA gösterilmeli mi kontrol eder (rate limiting)
        bool ShouldShowCaptcha(string ipAddress);

        // Form gönderimini kaydeder (IP bazlı sayaç için)
        void RecordFormSubmission(string ipAddress);

        // Son gönderim sayısını getirir (ViewBag için)
        int GetRecentSubmissionCount(string ipAddress);
    }
}

