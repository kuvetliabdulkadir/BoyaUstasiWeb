using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Verilen hizmet (Boya, Badana, Tadilat vb.) varlığı.
    public class Service : BaseDisplayEntity
    {
        [Required(ErrorMessage = "Hizmet başlığı zorunludur")]
        [StringLength(100, ErrorMessage = "Başlık en fazla 100 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Kısa açıklama en fazla 300 karakter olabilir")]
        public string? ShortDescription { get; set; }

        [StringLength(2000)]
        public string? FullDescription { get; set; }

        public string? IconClass { get; set; } // Font Awesome veya Bootstrap icon sınıfı

        public string? ImagePath { get; set; }

        public string? PriceInfo { get; set; } // "30-50 TL/m²" gibi

        // Navigation property
        public ICollection<ServiceDetail>? Details { get; set; }
    }
}

