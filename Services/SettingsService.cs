using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Veritabanı tabanlı site ayarları yönetimi servisi
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(ApplicationDbContext context, ILogger<SettingsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await _context.SiteSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value;
        }

        public async Task<Dictionary<string, string?>> GetSettingsByGroupAsync(string group)
        {
            var settings = await _context.SiteSettings
                .AsNoTracking()
                .Where(s => s.Group == group)
                .ToListAsync();

            return settings
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        public async Task<Dictionary<string, string?>> GetAllSettingsAsync()
        {
            var settings = await _context.SiteSettings
                .AsNoTracking()
                .ToListAsync();

            return settings
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        // Tüm ayarları getirir (sync - Layout için)
        public Dictionary<string, string?> GetAllSettings()
        {
            var settings = _context.SiteSettings
                .AsNoTracking()
                .ToList();

            return settings
                .Where(s => !string.IsNullOrEmpty(s.Key))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.First().Value);
        }

        public async Task<bool> UpdateSettingAsync(string key, string? value)
        {
            try
            {
                var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
                if (setting == null)
                {
                    _logger.LogWarning("Ayar bulunamadı (Update): {Key}", key);
                    return false;
                }

                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ayar güncellendi: {Key}", key);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayar güncellenirken hata oluştu: {Key}", key);
                return false;
            }
        }

        public async Task<bool> UpdateOrCreateSettingAsync(string key, string? value, string group)
        {
            try
            {
                var setting = await _context.SiteSettings.FirstOrDefaultAsync(s => s.Key == key);
                if (setting == null)
                {
                    _logger.LogInformation("Yeni ayar oluşturuluyor: {Key}", key);
                    _context.SiteSettings.Add(new SiteSetting
                    {
                        Key = key,
                        Value = value,
                        Group = group,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    setting.Value = value;
                    setting.Group = group; // Grubu da güncelle (gerekirse)
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ayar güncellenirken/oluşturulurken hata oluştu: {Key}", key);
                return false;
            }
        }

        public async Task<PopupSetting?> GetActivePopupAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.PopupSettings
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Where(p => p.StartDate == null || p.StartDate <= now)
                .Where(p => p.EndDate == null || p.EndDate >= now)
                .FirstOrDefaultAsync();
        }
    }
}

