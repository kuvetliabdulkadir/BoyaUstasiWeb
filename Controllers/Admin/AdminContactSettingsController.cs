using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Data;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli "İletişim" sayfası ayarlarını yöneten kontrolcü.
    [Route("usta-panel-2024/ayarlar/iletisim")]
    public class AdminContactSettingsController : AdminBaseController
    {
        private readonly ISettingsService _settingsService;

        public AdminContactSettingsController(ISettingsService settingsService, ApplicationDbContext context, ISettingsService settingsServiceBase) 
            : base(context, settingsServiceBase)
        {
            _settingsService = settingsService;
        }

        // İletişim ayarlarını görüntüle
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var keys = new[] { "Phone", "WhatsApp", "Email", "Address", "WorkingHours", "Cities", "GoogleMapsEmbed", "CityDistrictsMap", "GooglePlacesAPIKey", "WhatsAppDefaultMessage" };
            var settings = await _settingsService.GetAllSettingsAsync();

            var vm = new ContactSettingsVm();
            // Ayarların ViewModel'e aktarılması
            vm.Phone = settings.GetValueOrDefault("Phone") ?? "";
            vm.WhatsApp = settings.GetValueOrDefault("WhatsApp") ?? "";
            vm.Email = settings.GetValueOrDefault("Email") ?? "";
            vm.Address = settings.GetValueOrDefault("Address") ?? "";
            vm.WorkingHours = settings.GetValueOrDefault("WorkingHours") ?? "";
            vm.Cities = settings.GetValueOrDefault("Cities") ?? "";
            vm.GoogleMapsEmbed = settings.GetValueOrDefault("GoogleMapsEmbed") ?? "";
            vm.WhatsAppDefaultMessage = settings.GetValueOrDefault("WhatsAppDefaultMessage") ?? "Merhaba, boya hizmeti hakkında bilgi almak istiyorum.";

            ViewBag.CityDistrictsMapJson = settings.GetValueOrDefault("CityDistrictsMap") ?? "";
            ViewBag.GooglePlacesAPIKey = settings.GetValueOrDefault("GooglePlacesAPIKey") ?? "";

            return View("~/Views/AdminSettings/Contact.cshtml", vm);
        }

        // İletişim ayarlarını kaydet
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactSettingsVm vm, [FromForm] string? cityDistrictsMapJson, [FromForm] string? googlePlacesAPIKey)
        {
            if (!ModelState.IsValid) return View("~/Views/AdminSettings/Contact.cshtml", vm);

            await _settingsService.UpdateOrCreateSettingAsync("Phone", vm.Phone, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("WhatsApp", vm.WhatsApp, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("Email", vm.Email, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("Address", vm.Address, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("WorkingHours", vm.WorkingHours, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("Cities", vm.Cities, "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("GoogleMapsEmbed", vm.GoogleMapsEmbed ?? "", "İletişim");
            await _settingsService.UpdateOrCreateSettingAsync("WhatsAppDefaultMessage", vm.WhatsAppDefaultMessage ?? "Merhaba, boya hizmeti hakkında bilgi almak istiyorum.", "İletişim");

            if (!string.IsNullOrWhiteSpace(cityDistrictsMapJson))
            {
                await _settingsService.UpdateOrCreateSettingAsync("CityDistrictsMap", cityDistrictsMapJson, "İletişim");
            }

            if (!string.IsNullOrWhiteSpace(googlePlacesAPIKey))
            {
                await _settingsService.UpdateOrCreateSettingAsync("GooglePlacesAPIKey", googlePlacesAPIKey, "İletişim");
            }

            TempData["Success"] = "İletişim ayarları güncellendi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
