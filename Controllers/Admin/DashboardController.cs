using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers.Admin
{
    // Admin paneli ana sayfa (Dashboard) kontrolcüsü. Özet verileri gösterir.
    [Route("usta-panel-2024")]
    public class DashboardController : AdminBaseController
    {
        public DashboardController(ApplicationDbContext context, ISettingsService settingsService)
            : base(context, settingsService)
        {
        }

        // Dashboard ana sayfası
        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Index()
        {
            // İstatistikleri hazırla
            ViewBag.ProjectCount = await _context.Projects.CountAsync();
            ViewBag.ReviewCount = await _context.Reviews.CountAsync();
            ViewBag.MessageCount = await _context.ContactMessages.CountAsync();
            ViewBag.UnreadMessageCount = await _context.ContactMessages.CountAsync(m => !m.IsRead);
            ViewBag.ColorCount = await _context.ColorPalettes.CountAsync();
            ViewBag.ServiceCount = await _context.Services.CountAsync();

            // Son mesajlar
            ViewBag.RecentMessages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .Take(5)
                .ToListAsync();

            // Son projeler
            ViewBag.RecentProjects = await _context.Projects
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
}

