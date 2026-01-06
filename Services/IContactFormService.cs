using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // İletişim formu işlemleri için servis arayüzü
    public interface IContactFormService
    {
        Task<(bool Success, string Message, string? WhatsAppLink)> ProcessContactFormAsync(ContactMessage model, IFormFile[]? attachments, string ipAddress, string userAgent, string? website);
        Task<List<string>> GetActiveServiceTitlesAsync();
        (List<string> Cities, Dictionary<string, List<string>> CityDistrictsMap, string DefaultCity) GetCityData(Dictionary<string, string?> settings);
    }
}
