using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models.ViewModels
{
    // Ana sayfa ayarları için ViewModel
    public class HomeSettingsVm
    {
        [Display(Name = "Ana Sayfa - Başlık")]
        public string HeroTitle { get; set; } = string.Empty;

        [Display(Name = "Ana Sayfa - Ekstra Alt Başlık")]
        public string HeroSubtitle { get; set; } = string.Empty;

        [Display(Name = "Ana Sayfa - Tecrübe Rozeti")]
        public string HeroExperienceBadge { get; set; } = string.Empty;

        [Display(Name = "Ana Sayfa - Açıklama")]
        public string HeroDescription { get; set; } = string.Empty;



        // Neden Biz Bölümü
        [Display(Name = "Neden Biz - Alt Başlık")]
        public string WhyUsSubtitle { get; set; } = string.Empty;

        [Display(Name = "Neden Biz - Başlık")]
        public string WhyUsTitle { get; set; } = string.Empty;

        [Display(Name = "Neden Biz - Açıklama")]
        public string WhyUsExperienceDescription { get; set; } = string.Empty;

        // Kart 1
        [Display(Name = "Kart 1 - Başlık")]
        public string WhyUsCard1Title { get; set; } = string.Empty;

        [Display(Name = "Kart 1 - Açıklama")]
        public string WhyUsCard1Desc { get; set; } = string.Empty;

        [Display(Name = "Kart 1 - İkon")]
        public string WhyUsCard1Icon { get; set; } = string.Empty;

        [Display(Name = "Kart 1 - Aktif")]
        public bool WhyUsCard1IsActive { get; set; } = true;

        // Kart 2
        [Display(Name = "Kart 2 - Başlık")]
        public string WhyUsCard2Title { get; set; } = string.Empty;

        [Display(Name = "Kart 2 - Açıklama")]
        public string TeamExperienceDescription { get; set; } = string.Empty;

        [Display(Name = "Kart 2 - İkon")]
        public string WhyUsCard2Icon { get; set; } = string.Empty;

        [Display(Name = "Kart 2 - Aktif")]
        public bool WhyUsCard2IsActive { get; set; } = true;

        // Kart 3
        [Display(Name = "Kart 3 - Başlık")]
        public string WhyUsCard3Title { get; set; } = string.Empty;

        [Display(Name = "Kart 3 - Açıklama")]
        public string WhyUsCard3Desc { get; set; } = string.Empty;

        [Display(Name = "Kart 3 - İkon")]
        public string WhyUsCard3Icon { get; set; } = string.Empty;

        [Display(Name = "Kart 3 - Aktif")]
        public bool WhyUsCard3IsActive { get; set; } = true;

        // Kart 4
        [Display(Name = "Kart 4 - Başlık")]
        public string WhyUsCard4Title { get; set; } = string.Empty;

        [Display(Name = "Kart 4 - Açıklama")]
        public string WhyUsCard4Desc { get; set; } = string.Empty;

        [Display(Name = "Kart 4 - İkon")]
        public string WhyUsCard4Icon { get; set; } = string.Empty;

        [Display(Name = "Kart 4 - Aktif")]
        public bool WhyUsCard4IsActive { get; set; } = true;

        // Hero İkonları
        [Display(Name = "Hero Özellik 1 - İkon")]
        public string HeroFeature1Icon { get; set; } = string.Empty;

        [Display(Name = "Hero Özellik 1 - Yazı")]
        public string HeroFeature1Text { get; set; } = string.Empty;

        [Display(Name = "Hero Özellik 2 - İkon")]
        public string HeroFeature2Icon { get; set; } = string.Empty;

        [Display(Name = "Hero Özellik 2 - Yazı")]
        public string HeroFeature2Text { get; set; } = string.Empty;

        [Display(Name = "Hero Özellik 3 - İkon")]
        public string HeroFeature3Icon { get; set; } = string.Empty;

        [Display(Name = "Hero Özellik 3 - Yazı")]
        public string HeroFeature3Text { get; set; } = string.Empty;

        [Display(Name = "Tecrübe Yılı")]
        public string ExperienceYears { get; set; } = string.Empty;

        [Display(Name = "Footer Kısa Açıklama")]
        public string FooterAboutShort { get; set; } = string.Empty;

        [Display(Name = "Hero Görsel - Başlık")]
        public string HeroVisualTitle { get; set; } = string.Empty;

        [Display(Name = "Hero Görsel - Alt Başlık")]
        public string HeroVisualSubtitle { get; set; } = string.Empty;

        [Display(Name = "CTA - Başlık")]
        public string CtaTitle { get; set; } = string.Empty;

        [Display(Name = "CTA - Açıklama")]
        public string CtaDescription { get; set; } = string.Empty;

        [Display(Name = "Projeler CTA - Başlık")]
        public string ProjectsCtaTitle { get; set; } = string.Empty;

        [Display(Name = "Projeler CTA - Açıklama")]
        public string ProjectsCtaDescription { get; set; } = string.Empty;

        // Sayfa Başlıkları
        [Display(Name = "İletişim Sayfası - Alt Başlık")]
        public string ContactPageHeaderSubtitle { get; set; } = string.Empty;

        [Display(Name = "Projeler Sayfası - Alt Başlık")]
        public string ProjectsPageHeaderSubtitle { get; set; } = string.Empty;

        [Display(Name = "Hizmetler Sayfası - Alt Başlık")]
        public string ServicesPageHeaderSubtitle { get; set; } = string.Empty;

        [Display(Name = "Simülatör Sayfası - Alt Başlık")]
        public string SimulatorPageHeaderSubtitle { get; set; } = string.Empty;

        // Hizmet Bölgeleri (About Page)
        [Display(Name = "Hizmet Bölgeleri - Alt Başlık")]
        public string ServiceAreasSubtitle { get; set; } = string.Empty;

        [Display(Name = "Hizmet Bölgeleri - Başlık")]
        public string ServiceAreasTitle { get; set; } = string.Empty;
    }
}

