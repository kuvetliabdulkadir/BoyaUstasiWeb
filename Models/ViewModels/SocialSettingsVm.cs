using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // Sosyal medya ayarları için ViewModel
    public class SocialSettingsVm
    {
        [Display(Name = "Facebook Linki")]
        public string? Facebook { get; set; }

        [Display(Name = "Instagram Linki")]
        public string? Instagram { get; set; }

        [Display(Name = "YouTube Linki")]
        public string? YouTube { get; set; }

        [Display(Name = "LinkedIn Linki")]
        public string? LinkedIn { get; set; }

        [Display(Name = "TikTok Linki")]
        public string? TikTok { get; set; }
    }
}

