using boya_usta_web.Data;
using boya_usta_web.Models;
using Microsoft.EntityFrameworkCore;

namespace boya_usta_web.Services
{
    public class ServicePageService : IServicePageService
    {
        private readonly ApplicationDbContext _context;

        public ServicePageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive)
                .Include(s => s.Details)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int id)
        {
            return await _context.Services
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);
        }

        public async Task<Service?> GetServiceBySlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            return await _context.Services
                .Include(s => s.Details)
                .FirstOrDefaultAsync(s => s.Slug == slug && s.IsActive);
        }
    }
}
