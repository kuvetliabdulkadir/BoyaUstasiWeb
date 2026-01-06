using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // İletişim formu mesajı varlığı.
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Ad Soyad 3-100 karakter arasında olmalıdır")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$", ErrorMessage = "Sadece harf giriniz")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [StringLength(15)]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "İl seçimi zorunludur")]
        public string? City { get; set; }

        [Required(ErrorMessage = "İlçe seçimi zorunludur")]
        [RegularExpression(@"^[a-zA-ZğüşıöçĞÜŞİÖÇ\s]+$", ErrorMessage = "Sadece harf giriniz")]
        public string? District { get; set; }

        [Required(ErrorMessage = "Hizmet türü seçimi zorunludur")]
        public string? ServiceType { get; set; }

        [Range(1, 100000, ErrorMessage = "Metrekare 1-100000 arasında olmalıdır")]
        public int? SquareMeters { get; set; }

        [Range(1, 99999, ErrorMessage = "Minimum metrekare 1-99999 arasında olmalıdır")]
        public int? MinSquareMeters { get; set; }

        [Range(1, 100000, ErrorMessage = "Maksimum metrekare 1-100000 arasında olmalıdır")]
        public int? MaxSquareMeters { get; set; }

        [Required(ErrorMessage = "Açıklama zorunludur")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Açıklama 10-2000 karakter arasında olmalıdır")]
        public string? Message { get; set; }

        public string? AttachmentPath { get; set; } 

        public string? AttachmentPaths { get; set; } 

        public bool IsRead { get; set; } = false;

        public bool IsReplied { get; set; } = false;

        public string? AdminNotes { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        public DateTime? RepliedAt { get; set; }
    }
}

