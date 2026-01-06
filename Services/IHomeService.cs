using boya_usta_web.Models;
using boya_usta_web.Models.ViewModels;

namespace boya_usta_web.Services
{
    public interface IHomeService
    {
        Task<HomeIndexViewModel> GetHomeViewModelAsync();
        Task<List<Statistic>> GetStatisticsAsync();
    }
}
