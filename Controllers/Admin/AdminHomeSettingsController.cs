using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Data; 

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli "Ana Sayfa" ayarlarını yöneten kontrolcü.
    [Route("usta-panel-2024/ayarlar/ana-sayfa")]
    public class AdminHomeSettingsController : AdminBaseController
    {
        private readonly ISettingsService _settingsService;

        public AdminHomeSettingsController(ISettingsService settingsService, ApplicationDbContext context, ISettingsService settingsServiceBase) 
            : base(context, settingsServiceBase)
        {
            _settingsService = settingsService;
        }

        // Ana sayfa ayarlarını görüntüle
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var keys = new[] {
                "HeroTitle", "HeroSubtitle", "HeroExperienceBadge", "HeroDescription", "WhyUsSubtitle", "WhyUsTitle", "WhyUsExperienceDescription",
                "WhyUsCard1Title", "WhyUsCard1Desc", "WhyUsCard1Icon", "WhyUsCard1IsActive",
                "WhyUsCard2Title", "TeamExperienceDescription", "WhyUsCard2Icon", "WhyUsCard2IsActive",
                "WhyUsCard3Title", "WhyUsCard3Desc", "WhyUsCard3Icon", "WhyUsCard3IsActive",
                "WhyUsCard4Title", "WhyUsCard4Desc", "WhyUsCard4Icon", "WhyUsCard4IsActive",
                "HeroFeature1Icon", "HeroFeature1Text", "HeroFeature2Icon", "HeroFeature2Text", "HeroFeature3Icon", "HeroFeature3Text",
                "ExperienceYears", "FooterAboutShort",
                "HeroVisualTitle", "HeroVisualSubtitle",
                "CtaTitle", "CtaDescription",
                "ProjectsCtaTitle", "ProjectsCtaDescription",
                "ContactPageHeaderSubtitle", "ProjectsPageHeaderSubtitle", "ServicesPageHeaderSubtitle", "SimulatorPageHeaderSubtitle",
                "ServiceAreasTitle", "ServiceAreasSubtitle"
            };
            
            var settings = await _settingsService.GetAllSettingsAsync();

            var vm = new HomeSettingsVm();
            vm.HeroTitle = settings.GetValueOrDefault("HeroTitle") ?? "Bursa'nın En Güvenilir Boya Ustası";
            vm.HeroSubtitle = settings.GetValueOrDefault("HeroSubtitle") ?? "";
            vm.HeroExperienceBadge = settings.GetValueOrDefault("HeroExperienceBadge") ?? "";
            vm.HeroDescription = settings.GetValueOrDefault("HeroDescription") ?? "";
            vm.WhyUsSubtitle = settings.GetValueOrDefault("WhyUsSubtitle") ?? "";
            
            vm.WhyUsTitle = settings.GetValueOrDefault("WhyUsTitle") ?? "";
            vm.WhyUsExperienceDescription = settings.GetValueOrDefault("WhyUsExperienceDescription") ?? "";

            vm.WhyUsCard1Title = settings.GetValueOrDefault("WhyUsCard1Title") ?? "";
            vm.WhyUsCard1Desc = settings.GetValueOrDefault("WhyUsCard1Desc") ?? "";
            vm.WhyUsCard1Icon = settings.GetValueOrDefault("WhyUsCard1Icon") ?? "bi bi-bucket-fill";
            vm.WhyUsCard1IsActive = settings.GetValueOrDefault("WhyUsCard1IsActive") != "false";

            vm.WhyUsCard2Title = settings.GetValueOrDefault("WhyUsCard2Title") ?? "";
            vm.TeamExperienceDescription = settings.GetValueOrDefault("TeamExperienceDescription") ?? "";
            vm.WhyUsCard2Icon = settings.GetValueOrDefault("WhyUsCard2Icon") ?? "bi bi-people-fill";
            vm.WhyUsCard2IsActive = settings.GetValueOrDefault("WhyUsCard2IsActive") != "false";

            vm.WhyUsCard3Title = settings.GetValueOrDefault("WhyUsCard3Title") ?? "";
            vm.WhyUsCard3Desc = settings.GetValueOrDefault("WhyUsCard3Desc") ?? "";
            vm.WhyUsCard3Icon = settings.GetValueOrDefault("WhyUsCard3Icon") ?? "bi bi-cash-coin";
            vm.WhyUsCard3IsActive = settings.GetValueOrDefault("WhyUsCard3IsActive") != "false";

            vm.WhyUsCard4Title = settings.GetValueOrDefault("WhyUsCard4Title") ?? "";
            vm.WhyUsCard4Desc = settings.GetValueOrDefault("WhyUsCard4Desc") ?? "";
            vm.WhyUsCard4Icon = settings.GetValueOrDefault("WhyUsCard4Icon") ?? "bi bi-shield-check";
            vm.WhyUsCard4IsActive = settings.GetValueOrDefault("WhyUsCard4IsActive") != "false";

            var heroFeature1Icon = settings.GetValueOrDefault("HeroFeature1Icon");
            vm.HeroFeature1Icon = !string.IsNullOrWhiteSpace(heroFeature1Icon) ? heroFeature1Icon : "bi bi-shield-check";
            var heroFeature1Text = settings.GetValueOrDefault("HeroFeature1Text");
            vm.HeroFeature1Text = !string.IsNullOrWhiteSpace(heroFeature1Text) ? heroFeature1Text : "2 Yıl Garanti";

            var heroFeature2Icon = settings.GetValueOrDefault("HeroFeature2Icon");
            vm.HeroFeature2Icon = !string.IsNullOrWhiteSpace(heroFeature2Icon) ? heroFeature2Icon : "bi bi-cash-coin";
            var heroFeature2Text = settings.GetValueOrDefault("HeroFeature2Text");
            vm.HeroFeature2Text = !string.IsNullOrWhiteSpace(heroFeature2Text) ? heroFeature2Text : "Uygun Fiyat";

            var heroFeature3Icon = settings.GetValueOrDefault("HeroFeature3Icon");
            vm.HeroFeature3Icon = !string.IsNullOrWhiteSpace(heroFeature3Icon) ? heroFeature3Icon : "bi bi-award";
            var heroFeature3Text = settings.GetValueOrDefault("HeroFeature3Text");
            vm.HeroFeature3Text = !string.IsNullOrWhiteSpace(heroFeature3Text) ? heroFeature3Text : "Premium Kalite";

            vm.ExperienceYears = settings.GetValueOrDefault("ExperienceYears") ?? "15";
            vm.FooterAboutShort = settings.GetValueOrDefault("FooterAboutShort") ?? "";
            vm.HeroVisualTitle = settings.GetValueOrDefault("HeroVisualTitle") ?? "Kaliteli Boya Hizmeti";
            vm.HeroVisualSubtitle = settings.GetValueOrDefault("HeroVisualSubtitle") ?? "Profesyonel Çözümler";
            
            vm.CtaTitle = settings.GetValueOrDefault("CtaTitle") ?? "Hemen Ücretsiz Keşif İçin İletişime Geçin";
            vm.CtaDescription = settings.GetValueOrDefault("CtaDescription") ?? "WhatsApp'tan yazın veya telefon ile arayın. Size en uygun çözümü sunalım.";
            
            vm.ProjectsCtaTitle = settings.GetValueOrDefault("ProjectsCtaTitle") ?? "Sizin projeniz de burada olabilir!";
            vm.ProjectsCtaDescription = settings.GetValueOrDefault("ProjectsCtaDescription") ?? "Ücretsiz keşif için iletişime geçin.";
            
            vm.ContactPageHeaderSubtitle = settings.GetValueOrDefault("ContactPageHeaderSubtitle") ?? "Ücretsiz keşif için iletişime geçin";
            vm.ProjectsPageHeaderSubtitle = settings.GetValueOrDefault("ProjectsPageHeaderSubtitle") ?? "Tamamladığımız işlerden örnekler";
            vm.ServicesPageHeaderSubtitle = settings.GetValueOrDefault("ServicesPageHeaderSubtitle") ?? "Profesyonel boya çözümleri ile yaşam alanlarınızı güzelleştiriyoruz";
            vm.SimulatorPageHeaderSubtitle = settings.GetValueOrDefault("SimulatorPageHeaderSubtitle") ?? "Duvarlarınızın yeni rengini boyamadan önce görün!";
            
            vm.ServiceAreasTitle = settings.GetValueOrDefault("ServiceAreasTitle") ?? "Bursa Genelinde Hizmet";
            vm.ServiceAreasSubtitle = settings.GetValueOrDefault("ServiceAreasSubtitle") ?? "Hizmet Bölgelerimiz";

            return View("~/Views/AdminSettings/Home.cshtml", vm);
        }

        // Ana sayfa ayarlarını kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(HomeSettingsVm vm)
        {
            if (!ModelState.IsValid) return View("~/Views/AdminSettings/Home.cshtml", vm);

            await _settingsService.UpdateOrCreateSettingAsync("HeroTitle", vm.HeroTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroSubtitle", vm.HeroSubtitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroExperienceBadge", vm.HeroExperienceBadge, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroDescription", vm.HeroDescription, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsSubtitle", vm.WhyUsSubtitle, "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("WhyUsTitle", vm.WhyUsTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsExperienceDescription", vm.WhyUsExperienceDescription, "Genel");

            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard1Title", vm.WhyUsCard1Title, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard1Desc", vm.WhyUsCard1Desc, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard1Icon", vm.WhyUsCard1Icon, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard1IsActive", vm.WhyUsCard1IsActive ? "true" : "false", "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard2Title", vm.WhyUsCard2Title, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("TeamExperienceDescription", vm.TeamExperienceDescription, "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard2Icon", vm.WhyUsCard2Icon, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard2IsActive", vm.WhyUsCard2IsActive ? "true" : "false", "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard3Title", vm.WhyUsCard3Title, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard3Desc", vm.WhyUsCard3Desc, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard3Icon", vm.WhyUsCard3Icon, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard3IsActive", vm.WhyUsCard3IsActive ? "true" : "false", "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard4Title", vm.WhyUsCard4Title, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard4Desc", vm.WhyUsCard4Desc, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard4Icon", vm.WhyUsCard4Icon, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("WhyUsCard4IsActive", vm.WhyUsCard4IsActive ? "true" : "false", "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature1Icon", string.IsNullOrWhiteSpace(vm.HeroFeature1Icon) ? null : vm.HeroFeature1Icon.Trim(), "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature1Text", string.IsNullOrWhiteSpace(vm.HeroFeature1Text) ? null : vm.HeroFeature1Text.Trim(), "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature2Icon", string.IsNullOrWhiteSpace(vm.HeroFeature2Icon) ? null : vm.HeroFeature2Icon.Trim(), "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature2Text", string.IsNullOrWhiteSpace(vm.HeroFeature2Text) ? null : vm.HeroFeature2Text.Trim(), "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature3Icon", string.IsNullOrWhiteSpace(vm.HeroFeature3Icon) ? null : vm.HeroFeature3Icon.Trim(), "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroFeature3Text", string.IsNullOrWhiteSpace(vm.HeroFeature3Text) ? null : vm.HeroFeature3Text.Trim(), "Ana Sayfa");

            await _settingsService.UpdateOrCreateSettingAsync("ExperienceYears", vm.ExperienceYears, "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("FooterAboutShort", vm.FooterAboutShort, "Genel");
            
            await _settingsService.UpdateOrCreateSettingAsync("HeroVisualTitle", vm.HeroVisualTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("HeroVisualSubtitle", vm.HeroVisualSubtitle, "Ana Sayfa");
            
            await _settingsService.UpdateOrCreateSettingAsync("CtaTitle", vm.CtaTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("CtaDescription", vm.CtaDescription, "Ana Sayfa");
            
            await _settingsService.UpdateOrCreateSettingAsync("ProjectsCtaTitle", vm.ProjectsCtaTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("ProjectsCtaDescription", vm.ProjectsCtaDescription, "Ana Sayfa");
            
            await _settingsService.UpdateOrCreateSettingAsync("ContactPageHeaderSubtitle", vm.ContactPageHeaderSubtitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("ProjectsPageHeaderSubtitle", vm.ProjectsPageHeaderSubtitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("ServicesPageHeaderSubtitle", vm.ServicesPageHeaderSubtitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("SimulatorPageHeaderSubtitle", vm.SimulatorPageHeaderSubtitle, "Ana Sayfa");
            
            await _settingsService.UpdateOrCreateSettingAsync("ServiceAreasTitle", vm.ServiceAreasTitle, "Ana Sayfa");
            await _settingsService.UpdateOrCreateSettingAsync("ServiceAreasSubtitle", vm.ServiceAreasSubtitle, "Ana Sayfa");

            TempData["Success"] = "Ana sayfa ayarları güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
