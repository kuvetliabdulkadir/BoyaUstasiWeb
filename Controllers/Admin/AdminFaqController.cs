using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli SSS (Sıkça Sorulan Sorular) yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/sss")]
    public class AdminFaqController : AdminBaseController
    {
        public AdminFaqController(ApplicationDbContext context, ISettingsService settingsService)
            : base(context, settingsService)
        {
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            // SSS'leri sıraya göre listele
            var faqs = await _context.Faqs
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            return View(faqs);
        }

        [HttpGet("ekle")]
        public IActionResult Create()
        {
            // Varsayılan değerlerle form aç
            return View(new Faq { IsActive = true, DisplayOrder = 0 });
        }

        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Faq model)
        {
            if (!ModelState.IsValid) return View(model);

            model.CreatedAt = DateTime.UtcNow;
            _context.Faqs.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "SSS eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound();
            return View(faq);
        }

        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Faq model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var faq = await _context.Faqs.FindAsync(id);
            if (faq == null) return NotFound();

            // Alanları güncelle
            faq.Question = model.Question;
            faq.Answer = model.Answer;
            faq.IsActive = model.IsActive;
            faq.DisplayOrder = model.DisplayOrder;
            faq.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["Success"] = "SSS güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<Faq>(_context, id, "SSS", nameof(Index));
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
                return await UpdateDisplayOrderAsync<Faq>(_context, orderMap, "SSS", nameof(Index));
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
            return await DeleteAsync<Faq>(_context, id, "SSS", nameof(Index));
        }
    }
}


