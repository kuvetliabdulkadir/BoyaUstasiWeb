using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Müşteri yorumu/değerlendirmesi varlığı.
    public class Review : BaseDisplayEntity
    {
        [Required(ErrorMessage = "Müşteri adı zorunludur")]
        [StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir")]
        public string CustomerName { get; set; } = string.Empty; // A.B. formatında görüntülenecek

        [StringLength(100)]
        public string? Location { get; set; } // Nilüfer, Osmangazi vb.

        [Required(ErrorMessage = "Yorum metni zorunludur")]
        [StringLength(500, ErrorMessage = "Yorum en fazla 500 karakter olabilir")]
        public string Comment { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Puan 1-5 arasında olmalıdır")]
        public int Rating { get; set; } = 5;

        public string? Source { get; set; } // Google, Sahibinden, WhatsApp vb.

        public string? SourceLogoPath { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;
    }
}

