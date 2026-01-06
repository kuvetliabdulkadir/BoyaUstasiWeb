using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using System.Diagnostics;

namespace boya_usta_web.Controllers
{
    // Ana sayfa ve statik sayfaları yöneten kontrolcü.
    public class HomeController : BaseController
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ISeoService _seoService;
        private readonly IHomeService _homeService;

        public HomeController(
            ILogger<HomeController> logger,
            ISettingsService settingsService,
            ISeoService seoService,
            IContactFormService contactFormService,
            IHomeService homeService)
            : base(settingsService, contactFormService)
        {
            _logger = logger;
            _seoService = seoService;
            _homeService = homeService;
        }

        // Ana sayfayı görüntüle (Projeler, Yorumlar, Hizmetler vb. ile)
        public async Task<IActionResult> Index()
        {
            // SEO Ayarları - Ana sayfa için veritabanındaki varsayılan değerler kullanılır
            // SetPageMeta çağrılmadığında SeoService otomatik olarak DB'den okur
            _seoService.SetPageMeta(ogType: "website");

            // Tüm verileri servis üzerinden çek
            var viewModel = await _homeService.GetHomeViewModelAsync();

            // ViewBag'e de set et (BaseController uyumluluğu veya view içi legacy kullanımlar için)
            ViewBag.Breadcrumbs = viewModel.Breadcrumbs;
            ViewBag.Services = viewModel.Services;
            ViewBag.FeaturedProjects = viewModel.FeaturedProjects;
            ViewBag.Reviews = viewModel.Reviews;
            ViewBag.Faqs = viewModel.Faqs;
            ViewBag.Popup = viewModel.Popup;
            ViewBag.Settings = viewModel.Settings;

            return View(viewModel);
        }

        // Hakkımızda sayfasını görüntüle
        public async Task<IActionResult> About()
        {
            // Sayfa yolu (Breadcrumb)
            ViewBag.Breadcrumbs = new List<(string Name, string Url)>
            {
                ("Ana Sayfa", "/"),
                ("Hakkımızda", "/hakkimizda")
            };

            await LoadSettingsAsync();
            ViewBag.Statistics = await _homeService.GetStatisticsAsync();

            return View();
        }

        // Gizlilik politikası sayfası yönlendirmesi
        public IActionResult Privacy()
        {
            return RedirectToAction("Privacy", "Legal");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Sentry Test - Hata izleme testi
        // UYARI: Bu endpoint'i production'da kaldırın veya yetkilendirme ekleyin!
        [Route("Home/SentryTest")]
        public IActionResult SentryTest()
        {
            _logger.LogInformation("Sentry test hatası oluşturuluyor...");
            throw new Exception("Bu Sentry test hatasıdır! 🎉 Eğer bu mesajı Sentry dashboard'da görüyorsanız, entegrasyon başarılı!");
        }
    }
}
