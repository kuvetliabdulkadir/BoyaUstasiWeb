using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Sıkça Sorulan Soru varlığı.
    public class Faq : BaseDisplayEntity
    {
        [Required(ErrorMessage = "Soru zorunludur")]
        [StringLength(300, ErrorMessage = "Soru en fazla 300 karakter olabilir")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cevap zorunludur")]
        [StringLength(2000, ErrorMessage = "Cevap en fazla 2000 karakter olabilir")]
        public string Answer { get; set; } = string.Empty;
    }
}

