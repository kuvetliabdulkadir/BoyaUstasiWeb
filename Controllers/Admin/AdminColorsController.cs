using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli renk paleti yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/renkler")]
    public class AdminColorsController : AdminBaseController
    {
        public AdminColorsController(ApplicationDbContext context, ISettingsService settingsService)
            : base(context, settingsService)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // Renkleri kategori ve sıraya göre listele
            var colors = await _context.ColorPalettes
                .OrderBy(c => c.Category)
                .ThenBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(colors);
        }

        [HttpGet("ekle")]
        public IActionResult Create()
        {
            // Varsayılan değerlerle form aç
            return View(new ColorPalette { IsActive = true, Category = "Nötr", HexCode = "#2563EB" });
        }

        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ColorPalette model)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.UtcNow;
            _context.ColorPalettes.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Renk eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var color = await _context.ColorPalettes.FindAsync(id);
            if (color == null) return NotFound();
            return View(color);
        }

        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ColorPalette model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var color = await _context.ColorPalettes.FindAsync(id);
            if (color == null) return NotFound();

            // Alanları güncelle
            color.Name = model.Name;
            color.HexCode = model.HexCode;
            color.Category = model.Category;
            color.IsActive = model.IsActive;
            color.DisplayOrder = model.DisplayOrder;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Renk güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<ColorPalette>(_context, id, "Renk", nameof(Index));
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
                return await UpdateDisplayOrderAsync<ColorPalette>(_context, orderMap, "Renk", nameof(Index));
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
            return await DeleteAsync<ColorPalette>(_context, id, "Renk", nameof(Index));
        }
    }
}


