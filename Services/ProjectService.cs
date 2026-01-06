using boya_usta_web.Data;
using boya_usta_web.Models;
using Microsoft.EntityFrameworkCore;

namespace boya_usta_web.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;

        public ProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllActiveProjectsAsync(string? category = null)
        {
            var query = _context.Projects
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            return await query
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<string>> GetProjectCategoriesAsync()
        {
            return await _context.Projects
                .Where(p => p.IsActive)
                .Select(p => p.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<Project?> GetProjectBySlugAsync(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            return await _context.Projects
                .Include(p => p.AdditionalImages)
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);
        }

        public async Task<List<Project>> GetSimilarProjectsAsync(int currentProjectId, string? category)
        {
            if (string.IsNullOrEmpty(category)) return new List<Project>();

            return await _context.Projects
                .Where(p => p.IsActive && p.Id != currentProjectId && p.Category == category)
                .OrderByDescending(p => p.CreatedAt)
                .Take(3)
                .ToListAsync();
        }
    }
}
