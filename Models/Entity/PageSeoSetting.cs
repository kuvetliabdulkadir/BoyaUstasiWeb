using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Sayfa bazlı SEO ayarları varlığı.
    public class PageSeoSetting : BaseDisplayEntity
    {
        [Required]
        [StringLength(200)]
        public string PagePath { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Sayfa Adı")]
        public string PageName { get; set; } = string.Empty;

        [StringLength(70)]
        [Display(Name = "Sayfa Başlığı")]
        public string? Title { get; set; }

        [StringLength(160)]
        [Display(Name = "Meta Açıklama")]
        public string? MetaDescription { get; set; }

        [StringLength(500)]
        [Display(Name = "Anahtar Kelimeler")]
        public string? Keywords { get; set; }

        [StringLength(300)]
        [Display(Name = "OG Görsel")]
        public string? OgImage { get; set; }

        [StringLength(50)]
        [Display(Name = "OG Tipi")]
        public string OgType { get; set; } = "website";

        [StringLength(50)]
        [Display(Name = "Robots")]
        public string Robots { get; set; } = "index, follow";

        public bool IsSystemPage { get; set; } = false;
    }
}
