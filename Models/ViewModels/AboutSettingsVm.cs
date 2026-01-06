using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // Hakkımızda sayfası ayarları için ViewModel
    public class AboutSettingsVm
    {
        [Display(Name = "Hakkımızda - Üst Başlık")]
        [Required(ErrorMessage = "Üst başlık zorunludur.")]
        public string AboutTitle { get; set; } = string.Empty;

        [Display(Name = "Hakkımızda - Metin")]
        [Required(ErrorMessage = "Ana metin zorunludur.")]
        public string AboutText { get; set; } = string.Empty;

        [Display(Name = "Hakkımızda - İkinci Paragraf")]
        public string? AboutText2 { get; set; }

        [Display(Name = "Tecrübe Rozeti - Yıl")]
        public string ExperienceBadgeYears { get; set; } = string.Empty;

        [Display(Name = "Tecrübe Rozeti - Etiket")]
        public string ExperienceBadgeLabel { get; set; } = string.Empty;

        [Display(Name = "Hikayemiz Bölümü - Resim Yolu")]
        public string? AboutImagePath { get; set; }

        [Display(Name = "Hikayemiz Bölümü - İkon")]
        public string? AboutImageIcon { get; set; }
        
        public IFormFile? AboutImage { get; set; }

        // Dinamik Özellikler Listesi
        public List<AboutFeatureItem> Features { get; set; } = new List<AboutFeatureItem>();

        // Değerlerimiz / Neden Biz? Bölümü
        [Display(Name = "Değerlerimiz - Başlık")]
        public string ValuesTitle { get; set; } = "Neden Biz?";

        [Display(Name = "Değerlerimiz - Alt Başlık")]
        public string ValuesSubtitle { get; set; } = "Değerlerimiz";

        // Dinamik Değerler Listesi
        public List<AboutValueItem> Values { get; set; } = new List<AboutValueItem>();

        // Geriye dönük uyumluluk için eski özellikler (migration için)
        [Display(Name = "Özellik 1 - Metin")]
        public string? AboutFeature1 { get; set; }

        [Display(Name = "Özellik 1 - İkon")]
        public string AboutFeature1Icon { get; set; } = "bi bi-check2-circle";

        [Display(Name = "Özellik 1 - Aktif")]
        public bool AboutFeature1IsActive { get; set; } = true;

        [Display(Name = "Özellik 2 - Metin")]
        public string? AboutFeature2 { get; set; }

        [Display(Name = "Özellik 2 - İkon")]
        public string AboutFeature2Icon { get; set; } = "bi bi-check2-circle";

        [Display(Name = "Özellik 2 - Aktif")]
        public bool AboutFeature2IsActive { get; set; } = true;

        [Display(Name = "Özellik 3 - Metin")]
        public string? AboutFeature3 { get; set; }

        [Display(Name = "Özellik 3 - İkon")]
        public string AboutFeature3Icon { get; set; } = "bi bi-check2-circle";

        [Display(Name = "Özellik 3 - Aktif")]
        public bool AboutFeature3IsActive { get; set; } = true;

        [Display(Name = "Özellik 4 - Metin")]
        public string? AboutFeature4 { get; set; }

        [Display(Name = "Özellik 4 - İkon")]
        public string AboutFeature4Icon { get; set; } = "bi bi-check2-circle";

        [Display(Name = "Özellik 4 - Aktif")]
        public bool AboutFeature4IsActive { get; set; } = true;
    }

    // Hakkımızda özellik maddesi
    public class AboutFeatureItem
    {
        [Display(Name = "ID")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Display(Name = "Metin")]
        public string? Text { get; set; }
        
        [Display(Name = "İkon")]
        public string Icon { get; set; } = "bi bi-check2-circle";
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }

    // Hakkımızda değer maddesi (Neden Biz?)
    public class AboutValueItem
    {
        [Display(Name = "ID")]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Display(Name = "Başlık")]
        public string? Title { get; set; }
        
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
        
        [Display(Name = "İkon")]
        public string Icon { get; set; } = "bi bi-heart-fill";
        
        [Display(Name = "Resim Yolu")]
        public string? ImagePath { get; set; }
        
        [Display(Name = "Aktif")]
        public bool IsActive { get; set; } = true;
    }
}

