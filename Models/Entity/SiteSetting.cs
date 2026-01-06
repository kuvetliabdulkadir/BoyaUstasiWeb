using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Genel site ayarları (Key-Value) varlığı.
    public class SiteSetting
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Value { get; set; }

        [StringLength(200)]
        public string? Description { get; set; }

        public string? Group { get; set; } // Genel, İletişim, Sosyal Medya vb.

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

