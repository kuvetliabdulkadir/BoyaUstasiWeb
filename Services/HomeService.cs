using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace boya_usta_web.Services
{
    public class HomeService : IHomeService
    {
        private readonly ApplicationDbContext _context;
        private readonly ISettingsService _settingsService;

        public HomeService(ApplicationDbContext context, ISettingsService settingsService)
        {
            _context = context;
            _settingsService = settingsService;
        }

        public async Task<HomeIndexViewModel> GetHomeViewModelAsync()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            var popup = await _settingsService.GetActivePopupAsync();

            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();

            var featuredProjects = await _context.Projects
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Where(r => r.IsActive)
                .OrderBy(r => r.DisplayOrder)
                .ToListAsync();

            var faqs = await _context.Faqs
                .Where(f => f.IsActive)
                .OrderBy(f => f.DisplayOrder)
                .ToListAsync();

            var breadcrumbs = new List<(string Name, string Url)>
            {
                ("Ana Sayfa", "/")
            };

            return new HomeIndexViewModel
            {
                Breadcrumbs = breadcrumbs,
                Services = services,
                FeaturedProjects = featuredProjects,
                Reviews = reviews,
                Faqs = faqs,
                Popup = popup,
                Settings = settings
            };
        }

        public async Task<List<Statistic>> GetStatisticsAsync()
        {
            return await _context.Statistics
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }
    }
}
