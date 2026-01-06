using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers
{
    // Tüm controller'lar için ortak işlevselliği sağlayan temel controller sınıfı
    public abstract class BaseController : Controller
    {
        protected readonly ISettingsService _settingsService;
        protected readonly IContactFormService _contactFormService;

        protected BaseController(
            ISettingsService settingsService,
            IContactFormService contactFormService)
        {
            _settingsService = settingsService;
            _contactFormService = contactFormService;
        }

        // Tüm ayarları yükler ve ViewBag.Settings'e atar
        protected async Task<Dictionary<string, string?>> LoadSettingsAsync()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            ViewBag.Settings = settings;
            return settings;
        }

        // Sayfa başlığı alt metnini ViewBag'e ayarlar
        protected void SetPageHeaderSubtitle(Dictionary<string, string?> settings, string key, string defaultValue = "")
        {
            ViewBag.ContactPageHeaderSubtitle = settings.GetSetting(key, defaultValue);
        }

        // Şehirleri, ilçe haritasını ve varsayılan şehri ViewBag'e ayarlar (ContactController için)
        protected void SetCitiesAndDistricts(Dictionary<string, string?> settings)
        {
            var data = _contactFormService.GetCityData(settings);

            ViewBag.Cities = data.Cities;
            ViewBag.CityDistrictsMap = data.CityDistrictsMap;
            ViewBag.DefaultCity = data.DefaultCity;
        }

        // Veritabanındaki aktif hizmetlerden hizmet türlerini ViewBag'e ayarlar
        protected async Task SetServiceTypesAsync()
        {
            ViewBag.ServiceTypes = await _contactFormService.GetActiveServiceTitlesAsync();
        }

        // İletişim görünümü için gerekli tüm ViewBag verilerini hazırlar
        protected virtual async Task PrepareContactViewAsync()
        {
            var settings = await LoadSettingsAsync();
            SetPageHeaderSubtitle(settings, "ContactPageHeaderSubtitle", "Ücretsiz keşif için iletişime geçin");
            SetCitiesAndDistricts(settings);
            await SetServiceTypesAsync();
        }
    }
}

