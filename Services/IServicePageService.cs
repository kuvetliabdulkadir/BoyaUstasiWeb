using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    public interface IServicePageService
    {
        Task<List<Service>> GetAllActiveServicesAsync();
        Task<Service?> GetServiceByIdAsync(int id);
        Task<Service?> GetServiceBySlugAsync(string slug);
    }
}
