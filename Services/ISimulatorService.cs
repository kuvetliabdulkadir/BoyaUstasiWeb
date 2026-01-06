using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    public interface ISimulatorService
    {
        Task<List<ColorPalette>> GetAllActiveColorsAsync();
    }
}
