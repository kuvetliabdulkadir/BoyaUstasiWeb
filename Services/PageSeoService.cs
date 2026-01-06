using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Sayfa bazlı SEO ayarlarını yöneten servis
    public class PageSeoService : IPageSeoService
    {
        private readonly ApplicationDbContext _context;

        // URL path eşleştirme tablosu - MVC route'ları ile SEO-friendly path'leri eşleştirir
        private static readonly Dictionary<string, string> PathAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            // MVC Default Routes -> SEO-Friendly Paths
            { "/services", "/hizmetler" },
            { "/services/index", "/hizmetler" },
            { "/projects", "/projeler" },
            { "/projects/index", "/projeler" },
            { "/contact", "/iletisim" },
            { "/contact/index", "/iletisim" },
            { "/home/about", "/hakkimizda" },
            { "/simulator", "/simulator" },
            { "/simulator/index", "/simulator" },
            { "/home", "/" },
            { "/home/index", "/" },
            
            // Tersi de olabilir - SEO-Friendly -> SEO-Friendly (zaten doğru)
            { "/hizmetler", "/hizmetler" },
            { "/projeler", "/projeler" },
            { "/iletisim", "/iletisim" },
            { "/hakkimizda", "/hakkimizda" },
        };

        public PageSeoService(ApplicationDbContext context)
        {
            _context = context;
        }

        // URL path'ini normalize eder ve SEO-friendly path'e çevirir
        private string NormalizePath(string? path)
        {
            // Null veya boş kontrolü
            if (path == null || string.IsNullOrWhiteSpace(path))
                return "/";

            // Küçük harfe çevir ve sondaki slash'ı kaldır
            path = path.ToLowerInvariant().TrimEnd('/');
            
            if (string.IsNullOrEmpty(path))
                return "/";

            // Alias tablosunda var mı kontrol et
            if (PathAliases.TryGetValue(path, out var normalizedPath))
                return normalizedPath;

            return path;
        }

        // Belirtilen sayfa yolu için SEO ayarlarını getirir (async)
        public async Task<PageSeoSetting?> GetPageSeoAsync(string? pagePath)
        {
            var normalizedPath = GetNormalizedPath(pagePath);

            try
            {
                return await _context.PageSeoSettings
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PagePath.ToLower() == normalizedPath && p.IsActive);
            }
            catch
            {
                return null;
            }
        }

        // Belirtilen sayfa yolu için SEO ayarlarını getirir (sync - Layout için)
        public PageSeoSetting? GetPageSeo(string? pagePath)
        {
            var normalizedPath = GetNormalizedPath(pagePath);

            try
            {
                return _context.PageSeoSettings
                    .AsNoTracking()
                    .FirstOrDefault(p => p.PagePath.ToLower() == normalizedPath && p.IsActive);
            }
            catch
            {
                return null;
            }
        }

        // Path'i normalize eder
        private string GetNormalizedPath(string? pagePath)
        {
            if (string.IsNullOrWhiteSpace(pagePath))
                pagePath = "/";

            return NormalizePath(pagePath);
        }

        // Tüm aktif sayfa SEO ayarlarını getirir
        public async Task<List<PageSeoSetting>> GetAllPageSeoAsync()
        {
            return await _context.PageSeoSettings
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync();
        }
    }
}

