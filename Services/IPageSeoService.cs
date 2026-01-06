using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Sayfa bazlı SEO ayarlarını yöneten servis arayüzü
    public interface IPageSeoService
    {
        // Belirtilen sayfa yolu için SEO ayarlarını getirir (async)
        Task<PageSeoSetting?> GetPageSeoAsync(string? pagePath);

        // Belirtilen sayfa yolu için SEO ayarlarını getirir (sync - Layout için)
        PageSeoSetting? GetPageSeo(string? pagePath);

        // Tüm aktif sayfa SEO ayarlarını getirir
        Task<List<PageSeoSetting>> GetAllPageSeoAsync();
    }
}

