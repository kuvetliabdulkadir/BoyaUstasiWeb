using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Site ayarlarını (veritabanı tabanlı) yöneten servis arayüzü
    public interface ISettingsService
    {
        Task<string?> GetSettingAsync(string key);
        Task<Dictionary<string, string?>> GetSettingsByGroupAsync(string group);
        Task<Dictionary<string, string?>> GetAllSettingsAsync();
        
        // Tüm ayarları getirir (sync - Layout için)
        Dictionary<string, string?> GetAllSettings();
        
        Task<bool> UpdateSettingAsync(string key, string? value);
        Task<bool> UpdateOrCreateSettingAsync(string key, string? value, string group);
        Task<PopupSetting?> GetActivePopupAsync();
    }
}

