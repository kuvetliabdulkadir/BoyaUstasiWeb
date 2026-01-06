using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers
{
    // İletişim sayfası ve form işlemlerini yöneten kontrolcü.
    public class ContactController : BaseController
    {
        private readonly ITurnstileService _turnstileService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(
            ISettingsService settingsService,
            IContactFormService contactFormService,
            ITurnstileService turnstileService,
            ILogger<ContactController> logger)
            : base(settingsService, contactFormService)
        {
            _turnstileService = turnstileService;
            _logger = logger;
        }

        // İletişim sayfasını görüntüle
        [HttpGet]
        public async Task<IActionResult> Index(string? serviceType)
        {
            var viewModel = await PrepareContactViewModelAsync();
            viewModel.PreselectedServiceType = serviceType;
            return View(viewModel);
        }

        // İletişim formunu işle (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("form")]
        public async Task<IActionResult> Submit([Bind(Prefix = "FormModel")] ContactMessage model, IFormFile[]? attachments, string? website)
        {
            if (model == null)
            {
                return RedirectToAction("Index");
            }

            // Captcha Kontrolü
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            if (_turnstileService.ShouldShowCaptcha(ipAddress))
            {
                var turnstileToken = Request.Form["cf-turnstile-response"].ToString();
                var isValid = await _turnstileService.ValidateTokenAsync(turnstileToken, ipAddress);
                if (!isValid)
                {
                    _logger.LogWarning("Turnstile API hatası. IP: {Ip}, Token: {Token}", ipAddress, turnstileToken);
                    ModelState.AddModelError("", "Güvenlik doğrulaması başarısız.");
                    var viewModel = await PrepareContactViewModelAsync();
                    viewModel.FormModel = model;
                    return View("Index", viewModel);
                }
            }

            var userAgent = Request.Headers["User-Agent"].ToString();
            var result = await _contactFormService.ProcessContactFormAsync(model, attachments, ipAddress, userAgent, website);

            if (!result.Success)
            {
                ModelState.AddModelError("", result.Message);
                var viewModel = await PrepareContactViewModelAsync();
                viewModel.FormModel = model;
                return View("Index", viewModel);
            }

            // Başarılı
            TempData["ContactName"] = model.FullName ?? "Değerli Müşterimiz";
            TempData["WhatsAppLink"] = result.WhatsAppLink;
            TempData["FormSubmitted"] = true;
            return RedirectToAction("ThankYou");
        }

        // Teşekkür sayfası görüntüle
        [HttpGet]
        public IActionResult ThankYou()
        {
            if (TempData["ContactName"] == null)
            {
                return RedirectToAction("Index");
            }
            
            ViewData["Title"] = "Mesajınız Gönderildi";
            ViewBag.ContactName = TempData["ContactName"];
            ViewBag.WhatsAppLink = TempData["WhatsAppLink"];
            return View();
        }

        private async Task<ContactIndexViewModel> PrepareContactViewModelAsync()
        {
            var settings = await LoadSettingsAsync();
            var cityData = _contactFormService.GetCityData(settings);
            var serviceTypes = await _contactFormService.GetActiveServiceTitlesAsync();
            var remoteIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            var viewModel = new ContactIndexViewModel
            {
                Settings = settings,
                Cities = cityData.Cities,
                CityDistrictsMap = cityData.CityDistrictsMap,
                DefaultCity = cityData.DefaultCity,
                ServiceTypes = serviceTypes,
                PageHeaderSubtitle = settings.GetSetting("ContactPageHeaderSubtitle", "İletişime Geçin"),
                Captcha = new CaptchaSettings
                {
                    Enabled = _turnstileService.IsEnabled,
                    SiteKey = _turnstileService.SiteKey,
                    ShouldShow = _turnstileService.ShouldShowCaptcha(remoteIp),
                    SubmissionCount = _turnstileService.GetRecentSubmissionCount(remoteIp),
                    ThresholdCount = _turnstileService.ThresholdCount
                }
            };

            // ViewBag desteği (Eski kod uyumluluğu için)
            ViewBag.Settings = settings;
            ViewBag.Cities = viewModel.Cities;
            ViewBag.CityDistrictsMap = viewModel.CityDistrictsMap;
            ViewBag.DefaultCity = viewModel.DefaultCity;
            ViewBag.ServiceTypes = viewModel.ServiceTypes;
            ViewBag.TurnstileEnabled = _turnstileService.IsEnabled;
            ViewBag.TurnstileSiteKey = _turnstileService.SiteKey;
            ViewBag.ShouldShowCaptcha = viewModel.Captcha.ShouldShow;

            return viewModel;
        }
    }
}
