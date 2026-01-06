using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace boya_usta_web.Models
{
    // İstatistik (Mutlu Müşteri, Tecrübe vb.) varlığı.
    public class Statistic : BaseDisplayEntityWithoutCreatedAt, IValidatableObject
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty; // "Mutlu Müşteri", "Yıl Tecrübe" vb.

        [StringLength(50)]
        public string? Value { get; set; } // "500+", "15", "%100" (opsiyonel)

        public string? IconClass { get; set; }

        public string? Suffix { get; set; } // "+", "Yıl", "%" gibi

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Değer veya Sonek alanlarından en az birinin dolu olması gerekir (Örn: Sadece "Bursa" soneki ile değer boş olabilir)
            if (string.IsNullOrWhiteSpace(Value) && string.IsNullOrWhiteSpace(Suffix))
            {
                yield return new ValidationResult(
                    "Değer veya Ek (Suffix) alanlarından en az biri dolu olmalıdır.",
                    new[] { nameof(Value), nameof(Suffix) }
                );
            }
        }
    }
}

