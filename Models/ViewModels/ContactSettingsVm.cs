using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // İletişim ayarları için ViewModel
    public class ContactSettingsVm
    {
        [Display(Name = "Telefon (Gösterim)")]
        public string Phone { get; set; } = string.Empty;

        [Display(Name = "WhatsApp Numarası")]
        [Required(ErrorMessage = "WhatsApp numarası gereklidir.")]
        public string WhatsApp { get; set; } = string.Empty;

        [Display(Name = "E-posta")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Adres")]
        public string Address { get; set; } = string.Empty;

        [Display(Name = "Çalışma Saatleri")]
        public string WorkingHours { get; set; } = string.Empty;

        [Display(Name = "Hizmet Bölgeleri")]
        public string ServiceAreas { get; set; } = string.Empty;

        [Display(Name = "Hizmet İlleri")]
        public string Cities { get; set; } = string.Empty;

        [Display(Name = "Google Maps Embed Kodu")]
        public string? GoogleMapsEmbed { get; set; }

        [Display(Name = "WhatsApp Varsayılan Mesajı")]
        [StringLength(500, ErrorMessage = "Mesaj en fazla 500 karakter olabilir.")]
        public string WhatsAppDefaultMessage { get; set; } = "Merhaba, boya hizmeti hakkında bilgi almak istiyorum.";
    }
}
