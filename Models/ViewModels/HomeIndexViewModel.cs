using boya_usta_web.Models;

namespace boya_usta_web.Models.ViewModels
{
    // Home Index sayfası için ViewModel
    // ViewBag kullanımını azaltmak ve type-safe kod için
    public class HomeIndexViewModel
    {
        // Breadcrumb listesi
        public List<(string Name, string Url)> Breadcrumbs { get; set; } = new()
        {
            ("Ana Sayfa", "/")
        };

        // Aktif hizmetler listesi
        public List<Service> Services { get; set; } = new();

        // Öne çıkan projeler (ilk 3)
        public List<Project> FeaturedProjects { get; set; } = new();

        // Aktif yorumlar (ilk 6)
        public List<Review> Reviews { get; set; } = new();

        // Aktif SSS (ilk 6)
        public List<Faq> Faqs { get; set; } = new();

        // Aktif popup ayarı
        public PopupSetting? Popup { get; set; }

        // Site ayarları dictionary
        public Dictionary<string, string?> Settings { get; set; } = new();
    }
}

