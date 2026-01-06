using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Models.ViewModels;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers
{
    // Renk simülatörü ve renk paleti görüntüleme kontrolcüsü.
    public class SimulatorController : BaseController
    {
        private readonly ISimulatorService _simulatorService;

        public SimulatorController(
            ISimulatorService simulatorService, 
            ISettingsService settingsService,
            IContactFormService contactFormService)
            : base(settingsService, contactFormService)
        {
            _simulatorService = simulatorService;
        }

        // Simülatör sayfasını görüntüle
        public async Task<IActionResult> Index()
        {
            // Renk paletleri kategorilere göre
            var colors = await _simulatorService.GetAllActiveColorsAsync();

            var colorsByCategory = colors.GroupBy(c => c.Category)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Ayarları yükle (BaseController'dan gelir)
            var settings = await LoadSettingsAsync();

            var viewModel = new SimulatorIndexViewModel
            {
                ColorsByCategory = colorsByCategory,
                AllColors = colors,
                Settings = settings,
                SimulatorImagePath = settings.GetSetting("SimulatorImagePath"),
                PageHeaderSubtitle = settings.GetSetting("SimulatorPageHeaderSubtitle", "Duvarlarınızın yeni rengini boyamadan önce görün.")
            };

            return View(viewModel);
        }

        // Renk paleti verilerini JSON olarak getir (Simülatör JS için)
        [HttpGet]
        public async Task<IActionResult> GetColors()
        {
            var colors = await _simulatorService.GetAllActiveColorsAsync();

            var result = colors.Select(c => new { c.Id, c.Name, c.HexCode, c.Category }).ToList();

            return Json(result);
        }
    }
}

