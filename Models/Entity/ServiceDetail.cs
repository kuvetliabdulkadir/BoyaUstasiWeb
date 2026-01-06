using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Hizmet detay maddesi varlığı (Hizmet içeriği listesi).
    public class ServiceDetail
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Detay metni en fazla 1000 karakter olabilir.")]
        public string DetailText { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;

        // Navigation property
        public Service? Service { get; set; }
    }
}

