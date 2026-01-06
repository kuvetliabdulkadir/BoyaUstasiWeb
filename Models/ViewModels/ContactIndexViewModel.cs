using boya_usta_web.Models;

namespace boya_usta_web.Models.ViewModels
{
    // İletişim sayfası için ana ViewModel
    // ViewBag kullanımını azaltmak ve tip güvenliği sağlamak için kullanılır
    public class ContactIndexViewModel
    {
        // Form modeli (POST için)
        public ContactMessage? FormModel { get; set; }

        // Form gönderilmeden önce seçili hizmet tipi (query string'den)
        public string? PreselectedServiceType { get; set; }

        // Site ayarları dictionary
        public Dictionary<string, string?> Settings { get; set; } = new();

        // İl listesi
        public List<string> Cities { get; set; } = new();

        // İl-İlçe mapping dictionary
        public Dictionary<string, List<string>> CityDistrictsMap { get; set; } = new();

        // Varsayılan şehir
        public string DefaultCity { get; set; } = "Bursa";

        // Hizmet tipleri listesi
        public List<string> ServiceTypes { get; set; } = new();

        // Sayfa başlık alt metni
        public string PageHeaderSubtitle { get; set; } = "Ücretsiz keşif için iletişime geçin";

        // CAPTCHA ayarları
        public CaptchaSettings Captcha { get; set; } = new();
    }

    // CAPTCHA ayarları için nested class
    public class CaptchaSettings
    {
        public bool Enabled { get; set; }
        public string SiteKey { get; set; } = string.Empty;
        public bool ShouldShow { get; set; }
        public int SubmissionCount { get; set; }
        public int ThresholdCount { get; set; }
    }
}

