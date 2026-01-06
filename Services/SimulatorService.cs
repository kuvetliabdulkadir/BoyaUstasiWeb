using boya_usta_web.Data;
using boya_usta_web.Models;
using Microsoft.EntityFrameworkCore;

namespace boya_usta_web.Services
{
    public class SimulatorService : ISimulatorService
    {
        private readonly ApplicationDbContext _context;

        public SimulatorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ColorPalette>> GetAllActiveColorsAsync()
        {
            return await _context.ColorPalettes
                .Where(c => c.IsActive)
                .OrderBy(c => c.Category)
                .ThenBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}
