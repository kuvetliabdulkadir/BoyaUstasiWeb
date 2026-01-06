using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli proje yönetimini sağlayan kontrolcü.
    [Route("usta-panel-2024/projeler")]
    public class AdminProjectsController : AdminBaseController
    {
        private readonly IImageService _imageService;

        public AdminProjectsController(ApplicationDbContext context, IImageService imageService, ISettingsService settingsService)
            : base(context, settingsService)
        {
            _imageService = imageService;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(projects);
        }

        [HttpGet("ekle")]
        public async Task<IActionResult> Create()
        {
            // Aktif hizmetleri kategori olarak kullan
            // Aktif hizmetleri kategori olarak kullan
            ViewBag.Services = await GetActiveServiceTitlesAsync();
            return View();
        }

        [HttpPost("ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Project model, IFormFile? beforeImage, IFormFile? afterImage, IFormFile? videoFile)
        {
            // Aktif hizmetleri kategori olarak kullan
            ViewBag.Services = await GetActiveServiceTitlesAsync();
            
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (beforeImage != null)
            {
                model.BeforeImagePath = await _imageService.SaveImageAsync(beforeImage, "projects");
            }

            if (afterImage != null)
            {
                model.AfterImagePath = await _imageService.SaveImageAsync(afterImage, "projects");
            }

            if (videoFile != null)
            {
                if (_imageService.ValidateVideo(videoFile, out var videoError))
                {
                    model.VideoPath = await _imageService.SaveVideoAsync(videoFile, "projects-video");
                }
                else
                {
                    ModelState.AddModelError("videoFile", videoError ?? "Geçersiz video.");
                    return View(model);
                }
            }

            model.Slug = model.Title.ToSlug();
            _context.Projects.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Proje başarıyla eklendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("duzenle/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _context.Projects
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
                return NotFound();

            // Aktif hizmetleri kategori olarak kullan
            ViewBag.Services = await GetActiveServiceTitlesAsync();

            return View(project);
        }

        [HttpPost("duzenle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project model, IFormFile? beforeImage, IFormFile? afterImage, IFormFile? videoFile)
        {
            if (id != model.Id)
                return NotFound();

            // Aktif hizmetleri kategori olarak kullan
            ViewBag.Services = await GetActiveServiceTitlesAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            // Alanları güncelle
            project.Title = model.Title;
            project.Slug = model.Title.ToSlug();
            project.Description = model.Description;
            project.Category = model.Category;
            project.Location = model.Location;
            project.PaintBrand = model.PaintBrand;
            project.PaintColor = model.PaintColor;
            project.DurationDays = model.DurationDays;
            project.IsActive = model.IsActive;
            project.IsFeatured = model.IsFeatured;
            project.DisplayOrder = model.DisplayOrder;
            project.VideoEmbedCode = model.VideoEmbedCode;
            project.UpdatedAt = DateTime.UtcNow;

            // Öncesi resmini güncelle
            if (beforeImage != null)
            {
                if (!string.IsNullOrEmpty(project.BeforeImagePath))
                    await _imageService.DeleteImageAsync(project.BeforeImagePath);
                project.BeforeImagePath = await _imageService.SaveImageAsync(beforeImage, "projects");
            }

            // Sonrası resmini güncelle
            if (afterImage != null)
            {
                if (!string.IsNullOrEmpty(project.AfterImagePath))
                    await _imageService.DeleteImageAsync(project.AfterImagePath);
                project.AfterImagePath = await _imageService.SaveImageAsync(afterImage, "projects");
            }

            // Videoyu güncelle
            if (videoFile != null)
            {
                if (_imageService.ValidateVideo(videoFile, out var videoError))
                {
                    if (!string.IsNullOrEmpty(project.VideoPath))
                        await _imageService.DeleteImageAsync(project.VideoPath); // Videoları da siler
                    
                    project.VideoPath = await _imageService.SaveVideoAsync(videoFile, "projects-video");
                }
                else
                {
                    ModelState.AddModelError("videoFile", videoError ?? "Geçersiz video.");
                    return View(model);
                }
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Proje başarıyla güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("sil/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _context.Projects
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Proje bulunamadı." });
                return NotFound();
            }

            try
            {
                // Resimleri diskten sil
                if (!string.IsNullOrEmpty(project.BeforeImagePath))
                    await _imageService.DeleteImageAsync(project.BeforeImagePath);
                if (!string.IsNullOrEmpty(project.AfterImagePath))
                    await _imageService.DeleteImageAsync(project.AfterImagePath);
                if (!string.IsNullOrEmpty(project.VideoPath))
                    await _imageService.DeleteImageAsync(project.VideoPath);

                if (project.AdditionalImages != null && project.AdditionalImages.Any())
                {
                    foreach (var img in project.AdditionalImages)
                    {
                        if (!string.IsNullOrWhiteSpace(img.ImagePath))
                            await _imageService.DeleteImageAsync(img.ImagePath);
                    }
                }

                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Proje başarıyla silindi." });

                TempData["Success"] = "Proje başarıyla silindi.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Proje silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("duzenle/{id:int}/galeri-ekle")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddGalleryImages(int id, List<IFormFile> images)
        {
            var project = await _context.Projects
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null) return NotFound();

            if (images == null || images.Count == 0)
            {
                TempData["Error"] = "Lütfen en az 1 görsel seçin.";
                return RedirectToAction(nameof(Edit), new { id });
            }

            var currentMaxOrder = project.AdditionalImages?.Any() == true
                ? project.AdditionalImages.Max(x => x.DisplayOrder)
                : 0;

            foreach (var file in images)
            {
                if (file == null || file.Length == 0) continue;
                if (!_imageService.ValidateImage(file))
                {
                    TempData["Error"] = "Seçilen dosyalardan biri geçersiz görsel.";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                var path = await _imageService.SaveImageAsync(file, "projects");
                if (string.IsNullOrWhiteSpace(path))
                {
                    TempData["Error"] = "Görsel kaydedilemedi.";
                    return RedirectToAction(nameof(Edit), new { id });
                }
                _context.ProjectImages.Add(new ProjectImage
                {
                    ProjectId = id,
                    ImagePath = path,
                    DisplayOrder = ++currentMaxOrder
                });
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Galeri görselleri eklendi.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost("galeri-sil/{imageId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGalleryImage(int imageId)
        {
            var img = await _context.ProjectImages.FindAsync(imageId);
            if (img == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Görsel bulunamadı." });
                return NotFound();
            }

            try
            {
                var projectId = img.ProjectId;
                if (!string.IsNullOrWhiteSpace(img.ImagePath))
                    await _imageService.DeleteImageAsync(img.ImagePath);

                _context.ProjectImages.Remove(img);
                await _context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Galeri görseli silindi." });

                TempData["Success"] = "Galeri görseli silindi.";
                return RedirectToAction(nameof(Edit), new { id = projectId });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Görsel silinirken bir hata oluştu: {ex.Message}";
                var projectId = img.ProjectId;
                return RedirectToAction(nameof(Edit), new { id = projectId });
            }
        }

        [HttpPost("video-sil/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Proje bulunamadı." });
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(project.VideoPath))
                {
                    await _imageService.DeleteImageAsync(project.VideoPath);
                    project.VideoPath = null;
                    await _context.SaveChangesAsync();
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Video başarıyla silindi." });

                TempData["Success"] = "Video başarıyla silindi.";
                return RedirectToAction(nameof(Edit), new { id });
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Video silinirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // Aktif/Pasif yap (AdminBaseController'dan)
        [HttpPost("aktif-pasif/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            return await ToggleActiveAsync<Project>(_context, id, "Proje", nameof(Index));
        }

        // Yardımcı metodlar
        private async Task<List<string>> GetActiveServiceTitlesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .Select(s => s.Title)
                .ToListAsync();
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
                return await UpdateDisplayOrderAsync<Project>(_context, orderMap, "Proje", nameof(Index));
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Sıralama verisi okunamadı: " + ex.Message });
                return RedirectToAction(nameof(Index));
            }
        }
    }
}

