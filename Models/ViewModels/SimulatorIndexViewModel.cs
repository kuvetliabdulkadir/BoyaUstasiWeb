using boya_usta_web.Models;

namespace boya_usta_web.Models.ViewModels
{
    // Simülatör sayfası için ViewModel
    public class SimulatorIndexViewModel
    {
        
        // Renk paletleri kategorilere göre gruplanmış
        public Dictionary<string, List<ColorPalette>> ColorsByCategory { get; set; } = new();

    
        // Tüm renk paletleri
        public List<ColorPalette> AllColors { get; set; } = new();

    
        // Site ayarları dictionary
        public Dictionary<string, string?> Settings { get; set; } = new();

        // Simülatör görsel yolu
        public string SimulatorImagePath { get; set; } = string.Empty;


        // Sayfa başlık alt metni
        public string PageHeaderSubtitle { get; set; } = "Duvarlarınızın yeni rengini boyamadan önce görün.";
    }
}

