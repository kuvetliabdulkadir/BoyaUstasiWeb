using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // Genel SEO ayarları için ViewModel
    public class SeoSettingsVm
    {
        // Temel SEO
        [Display(Name = "Site URL")]
        [Url(ErrorMessage = "Geçerli bir URL giriniz.")]
        public string SiteUrl { get; set; } = string.Empty;

        [Display(Name = "Site Adı")]
        [StringLength(60)]
        public string SiteName { get; set; } = string.Empty;

        [Display(Name = "Varsayılan Sayfa Başlığı")]
        [StringLength(70, ErrorMessage = "Başlık en fazla 70 karakter olmalıdır.")]
        public string DefaultTitle { get; set; } = string.Empty;

        [Display(Name = "Varsayılan Meta Açıklama")]
        [StringLength(160, ErrorMessage = "Meta açıklama en fazla 160 karakter olmalıdır.")]
        public string DefaultDescription { get; set; } = string.Empty;

        [Display(Name = "Anahtar Kelimeler")]
        [StringLength(200)]
        public string DefaultKeywords { get; set; } = string.Empty;

        // Open Graph
        [Display(Name = "OG Görsel URL")]
        public string? DefaultOgImage { get; set; }

        // Tema
        [Display(Name = "Tema Rengi (HEX)")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli HEX renk kodu giriniz. Örn: #2c3e50")]
        public string ThemeColor { get; set; } = "#2c3e50";

        [Display(Name = "Dil/Bölge")]
        public string Locale { get; set; } = "tr_TR";

        // Geo Tags (Yerel SEO)
        [Display(Name = "Bölge Kodu")]
        public string GeoRegion { get; set; } = "TR-16";

        [Display(Name = "Şehir")]
        public string GeoPlacename { get; set; } = "Bursa";

        [Display(Name = "Koordinatlar (Enlem)")]
        public string GeoLatitude { get; set; } = "40.1885";

        [Display(Name = "Koordinatlar (Boylam)")]
        public string GeoLongitude { get; set; } = "29.0610";

        // Robots
        [Display(Name = "Robots Direktifi")]
        public string RobotsDefault { get; set; } = "index, follow";

        // Analytics
        [Display(Name = "Google Analytics ID")]
        [StringLength(50, ErrorMessage = "Google Analytics ID en fazla 50 karakter olmalıdır.")]
        public string? GoogleAnalyticsId { get; set; }
    }
}
