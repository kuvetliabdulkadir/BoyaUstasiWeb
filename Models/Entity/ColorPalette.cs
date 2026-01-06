using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Renk simülatörü için renk paleti varlığı.
    public class ColorPalette : BaseDisplayEntity
    {
        [Required(ErrorMessage = "Renk adı zorunludur")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty; // "Pastel Mavi", "Antik Beyaz" vb.

        [Required(ErrorMessage = "HEX kodu zorunludur")]
        [StringLength(7)]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Geçerli HEX renk kodu giriniz (#FFFFFF)")]
        public string HexCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori zorunludur")]
        public string Category { get; set; } = string.Empty; // Sıcak, Soğuk, Nötr, Pastel
    }
}

