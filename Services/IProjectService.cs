using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    public interface IProjectService
    {
        Task<List<Project>> GetAllActiveProjectsAsync(string? category = null);
        Task<List<string>> GetProjectCategoriesAsync();
        Task<Project?> GetProjectByIdAsync(int id);
        Task<Project?> GetProjectBySlugAsync(string slug);
        Task<List<Project>> GetSimilarProjectsAsync(int currentProjectId, string? category);
    }
}
