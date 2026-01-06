using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli hizmet yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/hizmetler")]
    public class AdminServicesController : AdminBaseController
    {
        private readonly IImageService _imageService;

        public AdminServicesController(ApplicationDbContext context, IImageService imageService, ISettingsService settingsService)
            : base(context, settingsService)
        {
            _imageService = imageService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Include(s => s.Details)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            return View(services);
        }

        [HttpGet("ekle")]
        public IActionResult Create()
        {
            ViewBag.DetailsText = "";
            return View(new Service { IsActive = true, DisplayOrder = 0 });
        }

        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model, string? detailsText, IFormFile? image)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DetailsText = detailsText ?? "";
                return View(model);
            }

            if (image != null)
            {
                if (!_imageService.ValidateImage(image))
                {
                    ModelState.AddModelError("", "Geçersiz görsel dosyası.");
                    ViewBag.DetailsText = detailsText ?? "";
                    return View(model);
                }

                var path = await _imageService.SaveImageAsync(image, "services");
                if (string.IsNullOrWhiteSpace(path))
                {
                    ModelState.AddModelError("", "Görsel kaydedilemedi.");
                    ViewBag.DetailsText = detailsText ?? "";
                    return View(model);
                }
                model.ImagePath = path;
            }

            model.CreatedAt = DateTime.UtcNow;
            model.Slug = model.Title.ToSlug();
            model.Details = ParseDetails(detailsText);
            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hizmet eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            ViewBag.DetailsText = service.Details == null
                ? ""
                : string.Join(Environment.NewLine, service.Details.OrderBy(d => d.DisplayOrder).Select(d => d.DetailText));

            return View(service);
        }

        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service model, string? detailsText, IFormFile? image)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewBag.DetailsText = detailsText ?? "";
                return View(model);
            }

            var service = await _context.Services.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            // Alanları güncelle
            service.Title = model.Title;
            service.Slug = model.Title.ToSlug();
            service.ShortDescription = model.ShortDescription;
            service.FullDescription = model.FullDescription;
            service.IconClass = model.IconClass;
            service.PriceInfo = model.PriceInfo;
            service.IsActive = model.IsActive;
            service.DisplayOrder = model.DisplayOrder;
            service.UpdatedAt = DateTime.UtcNow;

            if (image != null)
            {
                if (!_imageService.ValidateImage(image))
                {
                    ModelState.AddModelError("", "Geçersiz görsel dosyası.");
                    ViewBag.DetailsText = detailsText ?? "";
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(service.ImagePath))
                    await _imageService.DeleteImageAsync(service.ImagePath);

                var path = await _imageService.SaveImageAsync(image, "services");
                if (string.IsNullOrWhiteSpace(path))
                {
                    ModelState.AddModelError("", "Görsel kaydedilemedi.");
                    ViewBag.DetailsText = detailsText ?? "";
                    return View(model);
                }
                service.ImagePath = path;
            }

            // Detayları yeniden yaz
            if (service.Details != null && service.Details.Any())
                _context.ServiceDetails.RemoveRange(service.Details);
            service.Details = ParseDetails(detailsText);

            await _context.SaveChangesAsync();
            

            TempData["Success"] = "Hizmet güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.Include(s => s.Details).FirstOrDefaultAsync(s => s.Id == id);
            if (service == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Hizmet bulunamadı." });
                return NotFound();
            }

            try
            {
                if (service.Details != null && service.Details.Any())
                    _context.ServiceDetails.RemoveRange(service.Details);
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Hizmet başarıyla silindi." });

                TempData["Success"] = "Hizmet silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Hizmet silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Hizmet görselini silme işlemi
        [HttpPost("resim-kaldir/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var service = await _context.Services.FirstOrDefaultAsync(s => s.Id == id);
            if (service == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(service.ImagePath))
            {
                // Sadece /uploads/ klasöründeki dosyaları sil
                if (service.ImagePath.StartsWith("/uploads/"))
                {
                    await _imageService.DeleteImageAsync(service.ImagePath);
                }
                service.ImagePath = null;
                service.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Hizmet görseli kaldırıldı.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<Service>(_context, id, "Hizmet", nameof(Index));
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
                return await UpdateDisplayOrderAsync<Service>(_context, orderMap, "Hizmet", nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sıralama verisi okunamadı: " + ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }

        private static List<ServiceDetail> ParseDetails(string? detailsText)
        {
            var list = new List<ServiceDetail>();
            if (string.IsNullOrWhiteSpace(detailsText)) return list;

            var lines = detailsText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Length > 1000 ? x.Substring(0, 1000) : x) // Maksimum 1000 karakter
                .ToList();

            for (var i = 0; i < lines.Count; i++)
            {
                list.Add(new ServiceDetail { DetailText = lines[i], DisplayOrder = i + 1 });
            }

            return list;
        }
    }
}


