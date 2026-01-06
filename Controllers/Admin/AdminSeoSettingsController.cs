using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Data;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli SEO ayarlarını yöneten kontrolcü.
    [Route("usta-panel-2024/ayarlar/seo")]
    public class AdminSeoSettingsController : AdminBaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<AdminSeoSettingsController> _logger;

        public AdminSeoSettingsController(
            ISettingsService settingsService, 
            ApplicationDbContext context, 
            ISettingsService settingsServiceBase,
            ILogger<AdminSeoSettingsController> logger) 
            : base(context, settingsServiceBase)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        // SEO sayfası yönlendirmesi
        // Şimdilik Genel ayarlara yönlendiriyoruz

        [HttpGet("")]
        public IActionResult Index()
        {
            // Ana SEO sayfası şimdilik genel ayarlara gider
            return RedirectToAction(nameof(General));
        }

        [HttpGet("genel")]
        public async Task<IActionResult> General()
        {
            var settings = await _settingsService.GetAllSettingsAsync();

            var vm = new SeoSettingsVm
            {
                SiteUrl = settings.GetValueOrDefault("SeoSiteUrl") ?? "",
                SiteName = settings.GetValueOrDefault("SeoSiteName") ?? "",
                DefaultTitle = settings.GetValueOrDefault("SeoDefaultTitle") ?? "",
                DefaultDescription = settings.GetValueOrDefault("SeoDefaultDescription") ?? "",
                DefaultKeywords = settings.GetValueOrDefault("SeoDefaultKeywords") ?? "",
                DefaultOgImage = settings.GetValueOrDefault("SeoDefaultOgImage") ?? "",
                ThemeColor = settings.GetValueOrDefault("SeoThemeColor") ?? "#2c3e50",
                Locale = settings.GetValueOrDefault("SeoLocale") ?? "tr_TR",
                GeoRegion = settings.GetValueOrDefault("SeoGeoRegion") ?? "",
                GeoPlacename = settings.GetValueOrDefault("SeoGeoPlacename") ?? "",
                GeoLatitude = settings.GetValueOrDefault("SeoGeoLatitude") ?? "",
                GeoLongitude = settings.GetValueOrDefault("SeoGeoLongitude") ?? "",
                RobotsDefault = settings.GetValueOrDefault("SeoRobotsDefault") ?? "index, follow",
                GoogleAnalyticsId = settings.GetValueOrDefault("SeoGoogleAnalyticsId") ?? ""
            };

            return View("~/Views/AdminSettings/Seo.cshtml", vm);
        }

        [HttpPost("genel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> General(SeoSettingsVm vm)
        {
            if (!ModelState.IsValid) return View("~/Views/AdminSettings/Seo.cshtml", vm);

            await _settingsService.UpdateOrCreateSettingAsync("SeoSiteUrl", vm.SiteUrl, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoSiteName", vm.SiteName, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoDefaultTitle", vm.DefaultTitle, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoDefaultDescription", vm.DefaultDescription, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoDefaultKeywords", vm.DefaultKeywords, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoDefaultOgImage", vm.DefaultOgImage, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoThemeColor", vm.ThemeColor, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoLocale", vm.Locale, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoGeoRegion", vm.GeoRegion, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoGeoPlacename", vm.GeoPlacename, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoGeoLatitude", vm.GeoLatitude, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoGeoLongitude", vm.GeoLongitude, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoRobotsDefault", vm.RobotsDefault, "SEO");
            await _settingsService.UpdateOrCreateSettingAsync("SeoGoogleAnalyticsId", vm.GoogleAnalyticsId, "SEO");

            TempData["Success"] = "Genel SEO ayarları başarıyla güncellendi.";
            return RedirectToAction(nameof(General));
        }

        [HttpPost("analytics-sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGoogleAnalyticsId()
        {
            try
            {
                await _settingsService.UpdateOrCreateSettingAsync("SeoGoogleAnalyticsId", "", "SEO");
                TempData["Success"] = "Google Analytics ID başarıyla kaldırıldı.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google Analytics ID kaldırılırken hata oluştu");
                TempData["Error"] = "Google Analytics ID kaldırılırken bir hata oluştu.";
            }
            return RedirectToAction(nameof(General));
        }


    }
}
