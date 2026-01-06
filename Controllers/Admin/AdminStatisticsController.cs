using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli istatistik yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/istatistikler")]
    public class AdminStatisticsController : AdminBaseController
    {
        public AdminStatisticsController(ApplicationDbContext context, ISettingsService settingsService)
            : base(context, settingsService)
        {
        }

        // İstatistik listeleme
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var items = await _context.Statistics
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return View(items);
        }

        // Yeni istatistik ekleme sayfası
        [HttpGet("ekle")]
        public IActionResult Create()
        {
            return View(new Statistic
            {
                Title = "",
                Value = "0",
                Suffix = "",
                IconClass = "bi bi-star-fill",
                DisplayOrder = 0,
                IsActive = true
            });
        }

        // Yeni istatistik ekleme işlemi
        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Statistic model)
        {
            if (!ModelState.IsValid) return View(model);

            model.Value ??= "";
            model.UpdatedAt = DateTime.UtcNow;
            _context.Statistics.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "İstatistik eklendi.";
            return RedirectToAction(nameof(Index));
        }

        // İstatistik düzenleme sayfası
        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Statistics.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }

        // İstatistik düzenleme işlemi
        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Statistic model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var item = await _context.Statistics.FirstOrDefaultAsync(s => s.Id == id);
            if (item == null) return NotFound();

            model.Value ??= "";
            item.Title = model.Title;
            item.Value = model.Value;
            item.Suffix = model.Suffix;
            item.IconClass = model.IconClass;
            item.DisplayOrder = model.DisplayOrder;
            item.IsActive = model.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "İstatistik güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<Statistic>(_context, id, "İstatistik", nameof(Index));
        }

        // Sıralama güncelle (AdminBaseController'dan)
        [HttpPost("sira-guncelle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDisplayOrder([FromBody] System.Text.Json.JsonElement data)
        {
            try
            {
                var json = data.GetRawText();
                var orderMapString = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json);
                var orderMap = orderMapString?.ToDictionary(k => int.Parse(k.Key), v => v.Value) ?? new Dictionary<int, int>();
                return await UpdateDisplayOrderAsync<Statistic>(_context, orderMap, "İstatistik", nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sıralama verisi okunamadı: " + ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }

        // Sil (AdminBaseController'dan)
        [HttpPost("sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            return await DeleteAsync<Statistic>(_context, id, "İstatistik", nameof(Index));
        }
    }
}


