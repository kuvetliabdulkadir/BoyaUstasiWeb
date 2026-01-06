using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli müşteri yorumları yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/yorumlar")]
    public class AdminReviewsController : AdminBaseController
    {
        private readonly IImageService _imageService;
        private readonly IGoogleReviewsService _googleReviewsService;
        private readonly ISettingsService _settingsService;

        public AdminReviewsController(
            ApplicationDbContext context, 
            IImageService imageService,
            IGoogleReviewsService googleReviewsService,
            ISettingsService settingsService)
            : base(context, settingsService)
        {
            _imageService = imageService;
            _googleReviewsService = googleReviewsService;
            _settingsService = settingsService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .OrderBy(r => r.DisplayOrder)
                .ThenByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View(reviews);
        }

        [HttpGet("ekle")]
        public IActionResult Create()
        {
            return View(new Review { Rating = 5, IsActive = true, ReviewDate = DateTime.UtcNow });
        }

        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review model, IFormFile? sourceLogo)
        {
            if (!ModelState.IsValid) return View(model);

            if (sourceLogo != null)
            {
                if (!_imageService.ValidateImage(sourceLogo))
                {
                    ModelState.AddModelError("", "Geçersiz logo dosyası.");
                    return View(model);
                }
                var path = await _imageService.SaveImageAsync(sourceLogo, "reviews");
                if (string.IsNullOrWhiteSpace(path))
                {
                    ModelState.AddModelError("", "Logo kaydedilemedi.");
                    return View(model);
                }
                model.SourceLogoPath = path;
            }

            model.CreatedAt = DateTime.UtcNow;
            _context.Reviews.Add(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Yorum eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("duzenle/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            return View(review);
        }

        [HttpPost("duzenle/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review model, IFormFile? sourceLogo)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();

            // Alanları güncelle
            review.CustomerName = model.CustomerName;
            review.Location = model.Location;
            review.Comment = model.Comment;
            review.Rating = model.Rating;
            review.Source = model.Source;
            review.IsActive = model.IsActive;
            review.DisplayOrder = model.DisplayOrder;
            review.ReviewDate = model.ReviewDate;

            if (sourceLogo != null)
            {
                if (!_imageService.ValidateImage(sourceLogo))
                {
                    ModelState.AddModelError("", "Geçersiz logo dosyası.");
                    return View(model);
                }

                if (!string.IsNullOrWhiteSpace(review.SourceLogoPath))
                    await _imageService.DeleteImageAsync(review.SourceLogoPath);

                var path = await _imageService.SaveImageAsync(sourceLogo, "reviews");
                if (string.IsNullOrWhiteSpace(path))
                {
                    ModelState.AddModelError("", "Logo kaydedilemedi.");
                    return View(model);
                }
                review.SourceLogoPath = path;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Yorum güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<Review>(_context, id, "Yorum", nameof(Index));
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
                return await UpdateDisplayOrderAsync<Review>(_context, orderMap, "Yorum", nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sıralama verisi okunamadı: " + ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }

        // Logo silme işlemi
        [HttpPost("logo-sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLogo(int id)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(id);
                if (review == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = "Yorum bulunamadı." });
                    return NotFound();
                }

                if (!string.IsNullOrWhiteSpace(review.SourceLogoPath))
                {
                    await _imageService.DeleteImageAsync(review.SourceLogoPath);
                    review.SourceLogoPath = null;
                    await _context.SaveChangesAsync();
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Logo başarıyla silindi." });

                TempData["Success"] = "Logo başarıyla silindi.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Logo silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        [HttpPost("sil/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Yorum bulunamadı." });
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(review.SourceLogoPath))
                {
                    await _imageService.DeleteImageAsync(review.SourceLogoPath);
                }

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Yorum başarıyla silindi." });

            TempData["Success"] = "Yorum silindi.";
            return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Yorum silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Google yorumları aktarma sayfası
        [HttpGet("google-aktar")]
        public IActionResult ImportGoogleReviews()
        {
            return View();
        }

        // Google yorumları aktarma işlemi (POST)
        [HttpPost("google-aktar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportGoogleReviews(string placeId, string? apiKey, string? jsonData)
        {
            try
            {
                List<Review> reviews;

                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    // JSON'dan import
                    reviews = await _googleReviewsService.ImportReviewsFromJsonAsync(jsonData);
                }
                else if (!string.IsNullOrWhiteSpace(placeId))
                {
                    // API key'i ayarlardan al veya parametre olarak kullan
                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        apiKey = await _settingsService.GetSettingAsync("GooglePlacesAPIKey");
                    }

                    if (string.IsNullOrWhiteSpace(apiKey))
                    {
                        TempData["Error"] = "Google Places API Key gerekli. Lütfen API key'i girin veya ayarlardan yapılandırın.";
                        return View();
                    }

                    // Google Places API'den çek
                    reviews = await _googleReviewsService.FetchReviewsFromGoogleAsync(placeId, apiKey);
                }
                else
                {
                    TempData["Error"] = "Place ID veya JSON verisi gerekli.";
                    return View();
                }

                if (!reviews.Any())
                {
                    TempData["Warning"] = "Hiç yorum bulunamadı.";
                    return View();
                }

                // Yorumları veritabanına ekle (duplicate kontrolü ile)
                var addedCount = 0;
                var skippedCount = 0;

                foreach (var review in reviews)
                {
                    // Aynı müşteri adı ve yorum metninin ilk 50 karakteri ile duplicate kontrolü
                    var commentPrefix = review.Comment.Length > 50 ? review.Comment.Substring(0, 50) : review.Comment;
                    var existingReview = await _context.Reviews
                        .FirstOrDefaultAsync(r => 
                            r.CustomerName == review.CustomerName && 
                            r.Comment.StartsWith(commentPrefix));

                    if (existingReview == null)
                    {
                        _context.Reviews.Add(review);
                        addedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"{addedCount} yorum başarıyla eklendi. {skippedCount} yorum zaten mevcut olduğu için atlandı.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
                return View();
            }
        }
    }
}


