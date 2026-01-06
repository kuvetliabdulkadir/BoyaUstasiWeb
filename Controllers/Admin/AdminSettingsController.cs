using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli genel ayarlar, görseller, popup ve sosyal medya gibi ayarları yöneten kontrolcü.
    [Route("usta-panel-2024/ayarlar")]
    public class AdminSettingsController : AdminBaseController
    {
        private readonly IImageService _imageService;
        private readonly ILogger<AdminSettingsController> _logger;
        private readonly ISettingsService _settingsService;

        public AdminSettingsController(
            ApplicationDbContext context, 
            IImageService imageService, 
            ILogger<AdminSettingsController> logger, 
            IWebHostEnvironment environment,
            ISettingsService settingsService,
            ISettingsService settingsServiceBase)
            : base(context, settingsServiceBase)
        {
            _imageService = imageService;
            _logger = logger;
            _settingsService = settingsService;
        }

        // Genel ayarlar sayfası görüntüleme
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Footer link ayarlarını otomatik oluştur (yoksa)
            await _settingsService.UpdateOrCreateSettingAsync("FooterLinkText", "", "Footer");
            await _settingsService.UpdateOrCreateSettingAsync("FooterLinkUrl", "", "Footer");
            
            var settings = await _context.SiteSettings
                .OrderBy(s => s.Group)
                .ThenBy(s => s.Key)
                .ToListAsync();

            var popup = await _context.PopupSettings
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync();

            ViewBag.Popup = popup;
            return View(settings);
        }

        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var setting = await _context.SiteSettings.FindAsync(id);
            if (setting == null) return NotFound();
            return View(setting);
        }

        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SiteSetting model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var setting = await _context.SiteSettings.FindAsync(id);
            if (setting == null) return NotFound();

            setting.Value = model.Value;
            setting.Description = model.Description;
            setting.Group = model.Group;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Ayar güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Popup ayarları görüntüleme
        [HttpGet("popup")]
        public async Task<IActionResult> Popup()
        {
            var popup = await _context.PopupSettings.OrderByDescending(p => p.UpdatedAt).FirstOrDefaultAsync();
            if (popup == null)
            {
                popup = new PopupSetting { Title = "Yeni Pop-up", IsActive = false, DelaySeconds = 5, DontShowAgainDays = 30 };
                _context.PopupSettings.Add(popup);
                await _context.SaveChangesAsync();
            }
            return View(popup);
        }

        // Popup ayarları kaydetme
        [HttpPost("popup")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Popup(PopupSetting model)
        {
            if (!ModelState.IsValid) return View(model);

            var popup = await _context.PopupSettings.FindAsync(model.Id);
            if (popup == null) return NotFound();

            popup.Title = model.Title;
            popup.Content = model.Content;
            popup.ButtonText = model.ButtonText;
            popup.ButtonUrl = model.ButtonUrl;
            popup.ImagePath = model.ImagePath;
            popup.IsActive = model.IsActive;
            popup.DelaySeconds = model.DelaySeconds;
            popup.DontShowAgainDays = model.DontShowAgainDays;
            popup.StartDate = model.StartDate;
            popup.EndDate = model.EndDate;
            popup.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Pop-up güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Görsel ayarlar görüntüleme
        [HttpGet("gorseller")]
        public async Task<IActionResult> Assets()
        {
            // Gerekli key'ler yoksa oluştur
            await _settingsService.UpdateOrCreateSettingAsync("LogoPath", "/images/logo.png", "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("OgImagePath", "/images/og-image.jpg", "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("SimulatorImagePath", "", "Genel");

            var settings = await _context.SiteSettings
                .Where(s => s.Key == "LogoPath" || s.Key == "OgImagePath" || s.Key == "SimulatorImagePath")
                .OrderBy(s => s.Key)
                .ToListAsync();

            return View(settings);
        }

        // Görsel ayarlar kaydetme
        [HttpPost("gorseller")]
        [ValidateAntiForgeryToken]
        [RequestFormLimits(
            MultipartBodyLengthLimit = 10 * 1024 * 1024, // 10 MB
            ValueLengthLimit = int.MaxValue,
            MultipartBoundaryLengthLimit = int.MaxValue,
            KeyLengthLimit = int.MaxValue,
            MultipartHeadersLengthLimit = int.MaxValue)]
        public async Task<IActionResult> Assets(IFormFile? logo, IFormFile? ogImage, IFormFile? simulatorImage)
        {
            // Güncellemeden önce ayarların mevcut olduğundan emin ol
            await _settingsService.UpdateOrCreateSettingAsync("LogoPath", "/images/logo.png", "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("OgImagePath", "/images/og-image.jpg", "Genel");
            await _settingsService.UpdateOrCreateSettingAsync("SimulatorImagePath", "", "Genel");

            var errors = new List<string>();

            // Logo yükleme
            if (logo != null)
            {
                if (_imageService.ValidateLogo(logo, out var logoError))
                {
                    var path = await _imageService.SaveLogoAsync(logo);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await _settingsService.UpdateOrCreateSettingAsync("LogoPath", path, "Genel");
                    }
                    else
                    {
                        errors.Add("Logo yüklenirken bir hata oluştu.");
                    }
                }
                else
                {
                    errors.Add($"Logo: {logoError}");
                }
            }

            // OG Image yükleme
            if (ogImage != null)
            {
                if (_imageService.ValidateOgImage(ogImage, out var ogError))
                {
                    var path = await _imageService.SaveOgImageAsync(ogImage);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await _settingsService.UpdateOrCreateSettingAsync("OgImagePath", path, "Genel");
                    }
                    else
                    {
                        errors.Add("OG görseli yüklenirken bir hata oluştu.");
                    }
                }
                else
                {
                    errors.Add($"OG Görseli: {ogError}");
                }
            }

            // Simulator Görsel yükleme
            if (simulatorImage != null)
            {
                if (_imageService.ValidateImage(simulatorImage))
                {
                    var path = await _imageService.SaveImageAsync(simulatorImage, "site");
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        await _settingsService.UpdateOrCreateSettingAsync("SimulatorImagePath", path, "Genel");
                    }
                    else
                    {
                        errors.Add("Simülatör görseli yüklenirken bir hata oluştu.");
                    }
                }
                else
                {
                    errors.Add("Simülatör görseli: Geçersiz dosya formatı. PNG, JPG, JPEG veya WebP formatı kullanın.");
                }
            }

            if (errors.Any())
            {
                TempData["Error"] = string.Join(" ", errors);
            }
            else if (logo != null || ogImage != null || simulatorImage != null)
            {
                TempData["Success"] = "Görseller başarıyla güncellendi.";
            }

            return RedirectToAction(nameof(Assets));
        }

        [HttpPost("gorseller/logo-sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo()
        {
            return await DeleteImageSetting("LogoPath", "Logo", "Genel");
        }

        [HttpPost("gorseller/og-image-sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOgImage()
        {
            return await DeleteImageSetting("OgImagePath", "OG görseli", "Genel");
        }

        [HttpPost("gorseller/simulator-sil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSimulatorImage()
        {
            return await DeleteImageSetting("SimulatorImagePath", "Simülatör görseli", "Genel");
        }

        private async Task<IActionResult> DeleteImageSetting(string settingKey, string displayName, string group)
        {
            try
            {
                await _settingsService.UpdateOrCreateSettingAsync(settingKey, "", group);
                
                var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == settingKey);
                
                if (setting != null)
                {
                    var oldValue = setting.Value;
                    
                    if (!string.IsNullOrEmpty(oldValue) && oldValue.StartsWith("/uploads/"))
                    {
                        try
                        {
                            await _imageService.DeleteImageAsync(oldValue);
                            _logger.LogInformation("{DisplayName} dosyası silindi: {Path}", displayName, oldValue);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "{DisplayName} dosyası silinirken hata oluştu: {Path}", displayName, oldValue);
                        }
                    }
                    
                    setting.Value = "";
                    setting.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"{displayName} başarıyla kaldırıldı.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{DisplayName} kaldırılırken hata oluştu", displayName);
                TempData["Error"] = $"{displayName} kaldırılırken bir hata oluştu.";
            }

            return RedirectToAction(nameof(Assets));
        }

        // Sosyal Medya Ayarları
        // Sosyal medya ayarları görüntüleme
        [HttpGet("sosyal")]
        public async Task<IActionResult> Social()
        {
            var vm = new SocialSettingsVm();
            var keys = new[] { "Facebook", "Instagram", "YouTube", "LinkedIn", "TikTok" };
            var settings = await _context.SiteSettings
                .Where(s => keys.Contains(s.Key))
                .ToListAsync();

            vm.Facebook = settings.FirstOrDefault(s => s.Key == "Facebook")?.Value ?? "";
            vm.Instagram = settings.FirstOrDefault(s => s.Key == "Instagram")?.Value ?? "";
            vm.YouTube = settings.FirstOrDefault(s => s.Key == "YouTube")?.Value ?? "";
            vm.LinkedIn = settings.FirstOrDefault(s => s.Key == "LinkedIn")?.Value ?? "";
            vm.TikTok = settings.FirstOrDefault(s => s.Key == "TikTok")?.Value ?? "";

            return View(vm);
        }

        // Sosyal medya ayarları kaydetme
        [HttpPost("sosyal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Social(SocialSettingsVm vm)
        {
            ModelState.Remove(nameof(vm.Facebook));
            ModelState.Remove(nameof(vm.Instagram));
            ModelState.Remove(nameof(vm.YouTube));
            ModelState.Remove(nameof(vm.LinkedIn));
            ModelState.Remove(nameof(vm.TikTok));

            if (!ModelState.IsValid) return View(vm);

            await _settingsService.UpdateOrCreateSettingAsync("Facebook", vm.Facebook?.Trim() ?? "", "Sosyal Medya");
            await _settingsService.UpdateOrCreateSettingAsync("Instagram", vm.Instagram?.Trim() ?? "", "Sosyal Medya");
            await _settingsService.UpdateOrCreateSettingAsync("YouTube", vm.YouTube?.Trim() ?? "", "Sosyal Medya");
            await _settingsService.UpdateOrCreateSettingAsync("LinkedIn", vm.LinkedIn?.Trim() ?? "", "Sosyal Medya");
            await _settingsService.UpdateOrCreateSettingAsync("TikTok", vm.TikTok?.Trim() ?? "", "Sosyal Medya");

            TempData["Success"] = "Sosyal medya ayarları güncellendi.";
            return RedirectToAction(nameof(Social));
        }
    }
}
