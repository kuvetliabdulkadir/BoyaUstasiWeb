using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // Sayfa SEO ayarları için ViewModel
    public class PageSeoSettingVm
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Sayfa yolu zorunludur.")]
        [StringLength(200)]
        [Display(Name = "Sayfa Yolu")]
        public string PagePath { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sayfa adı zorunludur.")]
        [StringLength(100)]
        [Display(Name = "Sayfa Adı")]
        public string PageName { get; set; } = string.Empty;

        [StringLength(70, ErrorMessage = "Başlık en fazla 70 karakter olmalıdır.")]
        [Display(Name = "Sayfa Başlığı (Title)")]
        public string? Title { get; set; }

        [StringLength(160, ErrorMessage = "Açıklama en fazla 160 karakter olmalıdır.")]
        [Display(Name = "Meta Açıklama")]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        [Display(Name = "Anahtar Kelimeler")]
        public string? Keywords { get; set; }

        [StringLength(300)]
        [Display(Name = "OG Görsel URL")]
        public string? OgImage { get; set; }

        [StringLength(50)]
        [Display(Name = "OG Tipi")]
        public string OgType { get; set; } = "website";

        [StringLength(50)]
        [Display(Name = "Robots Direktifi")]
        public string Robots { get; set; } = "index, follow";

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Sıra")]
        public int DisplayOrder { get; set; } = 0;

        public bool IsSystemPage { get; set; } = false;
    }

    // SEO ayarları listesi için ViewModel
    public class PageSeoListVm
    {
        public List<PageSeoSettingVm> Pages { get; set; } = new();
    }
}

