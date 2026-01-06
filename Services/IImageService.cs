namespace boya_usta_web.Services
{
    // Görsel yükleme, işleme ve silme işlemleri için servis arayüzü
    public interface IImageService
    {
        // Standart görsel kaydetme (Public)
        Task<string?> SaveImageAsync(IFormFile file, string folder, bool convertToWebP = true);
        
        // Güvenli görsel kaydetme (Private)
        Task<string?> SaveSecureImageAsync(IFormFile file, string folder, bool convertToWebP = true);
        
        // Güvenli görsel yolunu getirme
        string? GetSecureImagePath(string fileName, string folder);
        
        // Görsel validasyonu
        bool ValidateImage(IFormFile file);
        
        // Görsel silme
        Task<bool> DeleteImageAsync(string imagePath);
        
        // Yeniden boyutlandırma ve WebP dönüşümü
        Task<string?> ResizeAndConvertToWebPAsync(string imagePath, int maxWidth = 1920, int maxHeight = 1080);
        
        // Özel görsel tipleri için validasyon
        bool ValidateLogo(IFormFile file, out string? errorMessage);
        bool ValidateFavicon(IFormFile file, out string? errorMessage);
        bool ValidateOgImage(IFormFile file, out string? errorMessage);
        bool ValidateVideo(IFormFile file, out string? errorMessage);

        // Özel görsel tipleri için kaydetme
        Task<string?> SaveLogoAsync(IFormFile file);
        Task<string?> SaveFaviconAsync(IFormFile file);
        Task<string?> SaveOgImageAsync(IFormFile file);
        
        // Video kaydetme
        Task<string?> SaveVideoAsync(IFormFile file, string folder);
        
        // CDN URL helper
        string GetImageUrl(string? imagePath);
        

    }
}

