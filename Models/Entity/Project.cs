using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Proje (önce/sonra resimleri, video ve detaylar) varlığı.
    public class Project : BaseDisplayEntity
    {
        [Required(ErrorMessage = "Proje başlığı zorunludur")]
        [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Açıklama en fazla 1000 karakter olabilir")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Kategori zorunludur")]
        public string Category { get; set; } = string.Empty; // Ic Cephe, Dis Cephe, Ticari, Dekoratif

        public string? Location { get; set; } // Konum (İlçe/Mahalle)

        public string? PaintBrand { get; set; } // Kullanılan boya markası

        public string? PaintColor { get; set; } // Kullanılan renk

        public int? DurationDays { get; set; } // Tamamlanma süresi (gün)

        public string? BeforeImagePath { get; set; } // Önce fotoğrafı

        public string? AfterImagePath { get; set; } // Sonra fotoğrafı

        public string? VideoPath { get; set; } // Yüklenen video dosyasının yolu (MP4)
        
        public string? VideoEmbedCode { get; set; } // YouTube/Vimeo embed kodu veya URL'si

        public bool IsFeatured { get; set; } = false; // Ana sayfada gösterilsin mi

        // Navigation property
        public ICollection<ProjectImage>? AdditionalImages { get; set; }
    }
}

