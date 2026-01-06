using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Data;
using System.Text.Json;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli "Hakkımızda" sayfası ayarlarını yöneten kontrolcü.
    [Route("usta-panel-2024/ayarlar/hakkimizda")]
    public class AdminAboutSettingsController : AdminBaseController
    {
        private readonly ISettingsService _settingsService;
        private readonly IImageService _imageService;

        public AdminAboutSettingsController(
            ISettingsService settingsService, 
            ApplicationDbContext context, 
            ISettingsService settingsServiceBase,
            IImageService imageService) 
            : base(context, settingsServiceBase)
        {
            _settingsService = settingsService;
            _imageService = imageService;
        }

        // Hakkımızda ayarlarını görüntüle
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _settingsService.GetAllSettingsAsync();

            var vm = new AboutSettingsVm();
            vm.AboutTitle = settings.GetValueOrDefault("AboutTitle") ?? "";
            vm.AboutText = settings.GetValueOrDefault("AboutText") ?? "";
            vm.AboutText2 = settings.GetValueOrDefault("AboutText2") ?? "";
            vm.ExperienceBadgeYears = settings.GetValueOrDefault("ExperienceBadgeYears") ?? "";
            vm.ExperienceBadgeLabel = settings.GetValueOrDefault("ExperienceBadgeLabel") ?? "";
            vm.AboutImagePath = settings.GetValueOrDefault("AboutImagePath");
            vm.AboutImageIcon = settings.GetValueOrDefault("AboutImageIcon") ?? "bi bi-brush-fill";
            vm.ValuesTitle = settings.GetValueOrDefault("ValuesTitle") ?? "Neden Biz?";
            vm.ValuesSubtitle = settings.GetValueOrDefault("ValuesSubtitle") ?? "Değerlerimiz";

            var featuresJson = settings.GetValueOrDefault("AboutFeatures");
            if (!string.IsNullOrEmpty(featuresJson))
            {
                try
                {
                    vm.Features = JsonSerializer.Deserialize<List<AboutFeatureItem>>(featuresJson) ?? new List<AboutFeatureItem>();
                }
                catch
                {
                    vm.Features = new List<AboutFeatureItem>();
                }
            }
            else
            {
                vm.Features = new List<AboutFeatureItem>();
            }

            var valuesJson = settings.GetValueOrDefault("AboutValues");
            if (!string.IsNullOrEmpty(valuesJson))
            {
                try
                {
                    vm.Values = JsonSerializer.Deserialize<List<AboutValueItem>>(valuesJson) ?? new List<AboutValueItem>();
                }
                catch
                {
                    vm.Values = new List<AboutValueItem>();
                }
            }
            else
            {
                vm.Values = new List<AboutValueItem>();
            }

            return View("~/Views/AdminSettings/About.cshtml", vm);
        }

        // Hakkımızda ayarlarını kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AboutSettingsVm vm)
        {
            // Gereksiz model alanlarını temizle
            ModelState.Remove(nameof(vm.AboutText2));
            ModelState.Remove(nameof(vm.Features));
            ModelState.Remove(nameof(vm.Values));
            ModelState.Remove(nameof(vm.AboutImage));

            if (vm.Features == null) vm.Features = new List<AboutFeatureItem>();
            if (vm.Values == null) vm.Values = new List<AboutValueItem>();

            if (!ModelState.IsValid) return View("~/Views/AdminSettings/About.cshtml", vm);

            await _settingsService.UpdateOrCreateSettingAsync("AboutTitle", vm.AboutTitle, "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("AboutText", vm.AboutText, "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("AboutText2", vm.AboutText2 ?? "", "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("ExperienceBadgeYears", vm.ExperienceBadgeYears, "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("ExperienceBadgeLabel", vm.ExperienceBadgeLabel, "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("AboutImageIcon", vm.AboutImageIcon ?? "bi bi-brush-fill", "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("ValuesTitle", vm.ValuesTitle, "Hakkımızda");
            await _settingsService.UpdateOrCreateSettingAsync("ValuesSubtitle", vm.ValuesSubtitle, "Hakkımızda");

            // Resim yükleme
            if (vm.AboutImage != null)
            {
                var currentPath = await _settingsService.GetSettingAsync("AboutImagePath");
                if (!string.IsNullOrEmpty(currentPath))
                {
                    await _imageService.DeleteImageAsync(currentPath);
                }
                
                var newPath = await _imageService.SaveImageAsync(vm.AboutImage, "about");
                await _settingsService.UpdateOrCreateSettingAsync("AboutImagePath", newPath, "Hakkımızda");
                vm.AboutImagePath = newPath;
            }

            // Özellikleri JSON olarak kaydet
            var featuresJson = JsonSerializer.Serialize(vm.Features);
            await _settingsService.UpdateOrCreateSettingAsync("AboutFeatures", featuresJson, "Hakkımızda");

            // Değerleri JSON olarak kaydet
            var valuesJson = JsonSerializer.Serialize(vm.Values);
            await _settingsService.UpdateOrCreateSettingAsync("AboutValues", valuesJson, "Hakkımızda");

            TempData["Success"] = "Hakkımızda ayarları güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Hakkımızda resmini sil
        [HttpPost("resim-sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage()
        {
            var path = await _settingsService.GetSettingAsync("AboutImagePath");
            if (!string.IsNullOrEmpty(path))
            {
                await _imageService.DeleteImageAsync(path);
                await _settingsService.UpdateOrCreateSettingAsync("AboutImagePath", "", "Hakkımızda");
            }
            TempData["Success"] = "Resim silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
