using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers
{
    // Hizmetleri listeleme ve detaylarını görüntüleme kontrolcüsü.
    public class ServicesController : BaseController
    {
        private readonly IServicePageService _servicePageService;

        public ServicesController(
            IServicePageService servicePageService, 
            ISettingsService settingsService,
            IContactFormService contactFormService)
            : base(settingsService, contactFormService)
        {
            _servicePageService = servicePageService;
        }

        // Hizmetleri listele
        public async Task<IActionResult> Index()
        {
            var services = await _servicePageService.GetAllActiveServicesAsync();

            // Ayarları yükle (BaseController'dan gelir)
            var settings = await LoadSettingsAsync();
            
            // Sayfa başlıklarını ayarlardan al
            ViewBag.ServicesPageHeaderSubtitle = settings.GetSetting("ServicesPageHeaderSubtitle", "Profesyonel boya çözümleri ile yaşam alanlarınızı güzelleştiriyoruz");

            return View(services);
        }

        // Hizmet detaylarını görüntüle (ID ile)
        public async Task<IActionResult> Detail(int id)
        {
            var service = await _servicePageService.GetServiceByIdAsync(id);

            if (service == null)
                return NotFound();

            // Eğer slug varsa ve URL'de ID kullanılmışsa slug'lı URL'e yönlendir (SEO için)
            if (!string.IsNullOrEmpty(service.Slug))
                return RedirectToRoute("service-detail", new { slug = service.Slug });

            await LoadSettingsAsync();
            return View(service);
        }

        // Hizmet detaylarını görüntüle (Slug/URL ile for SEO)
        public async Task<IActionResult> DetailBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var service = await _servicePageService.GetServiceBySlugAsync(slug);

            if (service == null)
                return NotFound();

            await LoadSettingsAsync();
            return View("Detail", service);
        }
    }
}

