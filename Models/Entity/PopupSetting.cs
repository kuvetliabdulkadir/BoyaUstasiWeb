using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Açılır pencere (Popup) ayarları varlığı.
    public class PopupSetting : BaseEntity
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty; // "İlk Müşterilere %10 İndirim!"

        [StringLength(500)]
        public string? Content { get; set; }

        [StringLength(50)]
        public string? ButtonText { get; set; } = "Teklif Al";

        public string? ButtonUrl { get; set; }

        public string? ImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        public int DelaySeconds { get; set; } = 5; // Kaç saniye sonra gösterilecek

        public int DontShowAgainDays { get; set; } = 30; // Kaç gün gösterilmeyecek

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

