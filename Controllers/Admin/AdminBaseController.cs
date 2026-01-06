using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using boya_usta_web.Data;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Models;
using boya_usta_web.Helpers;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    public abstract class AdminBaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly ISettingsService _settingsServiceBase;

        protected AdminBaseController(ApplicationDbContext context, ISettingsService settingsServiceBase)
        {
            _context = context;
            _settingsServiceBase = settingsServiceBase;
        }

        // Admin panel yolu - tüm admin View'larında kullanılır
        protected const string AdminPath = "/usta-panel-2024";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Admin path'i ViewBag'e set et (View'larda kullanılmak üzere)
            ViewBag.AdminPath = AdminPath;
            
            try
            {
                // Senkron bloklamayı küçük tutuyoruz; admin layout badge için tek sayı.
                ViewBag.UnreadMessageCount = _context.ContactMessages.Count(m => !m.IsRead);
            }
            catch
            {
                // Admin ekranı kırılmasın; badge gelmez ama sayfa açılır.
            }

            base.OnActionExecuting(context);
        }

        // Generic ToggleActive metodu - IDisplayEntity türevi entity'ler için
        protected async Task<IActionResult> ToggleActiveAsync<T>(ApplicationDbContext context, int id, string entityName, string redirectAction = "Index") where T : class, IDisplayEntity
        {
            try
            {
                var entity = await context.Set<T>().FindAsync(id);
                if (entity == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Json(new { success = false, message = $"{entityName} bulunamadı." });
                    return NotFound();
                }

                entity.IsActive = !entity.IsActive;
                
                // UpdatedAt varsa güncelle
                if (entity is BaseAuditableEntity auditableEntity)
                {
                    auditableEntity.UpdatedAt = DateTime.UtcNow;
                }
                else if (entity is BaseDisplayEntityWithoutCreatedAt withoutCreatedAt)
                {
                    withoutCreatedAt.UpdatedAt = DateTime.UtcNow;
                }

                await context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, isActive = entity.IsActive, message = $"{entityName} {(entity.IsActive ? "aktif" : "pasif")} yapıldı." });

                TempData["Success"] = $"{entityName} {(entity.IsActive ? "aktif" : "pasif")} yapıldı.";
                return RedirectToAction(redirectAction);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Durum değiştirilirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(redirectAction);
            }
        }

        // Generic UpdateDisplayOrder metodu - IDisplayEntity türevi entity'ler için
        protected async Task<IActionResult> UpdateDisplayOrderAsync<T>(ApplicationDbContext context, Dictionary<int, int> orderMap, string entityName, string redirectAction = "Index") where T : class, IDisplayEntity
        {
            try
            {
                foreach (var kvp in orderMap)
                {
                    var entity = await context.Set<T>().FindAsync(kvp.Key);
                    if (entity != null)
                    {
                        entity.DisplayOrder = kvp.Value;
                        
                        // UpdatedAt varsa güncelle
                        if (entity is BaseAuditableEntity auditableEntity)
                        {
                            auditableEntity.UpdatedAt = DateTime.UtcNow;
                        }
                        else if (entity is BaseDisplayEntityWithoutCreatedAt withoutCreatedAt)
                        {
                            withoutCreatedAt.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                }
                await context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Sıralama güncellendi." });

                TempData["Success"] = "Sıralama güncellendi.";
                return RedirectToAction(redirectAction);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"Sıralama güncellenirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(redirectAction);
            }
        }

        // Generic Delete metodu - BaseEntity türevi entity'ler için
        protected async Task<IActionResult> DeleteAsync<T>(ApplicationDbContext context, int id, string entityName, string redirectAction = "Index") where T : BaseEntity
        {
            var entity = await context.Set<T>().FindAsync(id);
            if (entity == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"{entityName} bulunamadı." });
                return NotFound();
            }

            try
            {
                context.Set<T>().Remove(entity);
                await context.SaveChangesAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = $"{entityName} başarıyla silindi." });

                TempData["Success"] = $"{entityName} silindi.";
                return RedirectToAction(redirectAction);
            }
            catch (Exception ex)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = $"Hata: {ex.Message}" });

                TempData["Error"] = $"{entityName} silinirken bir hata oluştu: {ex.Message}";
                return RedirectToAction(redirectAction);
            }
        }

        // Tüm slug'ları güncelle (Eski kayıtlar için)
        [HttpPost("fix-slugs")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FixSlugs()
        {
            // _context artık doğrudan erişilebilir
            var db = _context;
            if (db == null) return Json(new { success = false, message = "Veritabanı bağlantısı kurulamadı." });

            var services = await db.Services.ToListAsync();
            foreach (var s in services)
            {
                if (string.IsNullOrEmpty(s.Slug))
                    s.Slug = s.Title.ToSlug();
            }

            var projects = await db.Projects.ToListAsync();
            foreach (var p in projects)
            {
                if (string.IsNullOrEmpty(p.Slug))
                    p.Slug = p.Title.ToSlug();
            }

            await db.SaveChangesAsync();
            return Json(new { success = true, message = "Tüm slug'lar güncellendi." });
        }
    }
}

