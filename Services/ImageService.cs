using SkiaSharp;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using boya_usta_web.Data;
using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Görsel işleme, yükleme, yeniden boyutlandırma ve optimize etme servisi
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageService> _logger;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext? _context;

        // İzin verilen dosya uzantıları
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        // Maksimum dosya boyutu (5 MB)
        private readonly long _maxFileSize = 5 * 1024 * 1024;

        public ImageService(
            IWebHostEnvironment environment,
            ILogger<ImageService> logger,
            IConfiguration configuration,
            IServiceProvider? serviceProvider = null)
        {
            _environment = environment;
            _logger = logger;
            _configuration = configuration;
            // DbContext'i opsiyonel olarak al (Circular dependency riskini azaltmak için)
            _context = serviceProvider?.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
        }

        // Temel görsel validasyonu (Uzantı, boyut ve MIME type kontrolü)
        public bool ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            // Boyut kontrolü
            if (file.Length > _maxFileSize)
            {
                _logger.LogWarning("Dosya boyutu çok büyük: {Size} bytes", file.Length);
                return false;
            }

            // Uzantı kontrolü
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("Geçersiz dosya uzantısı: {Extension}", extension);
                return false;
            }

            // MIME type kontrolü
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLower()))
            {
                _logger.LogWarning("Geçersiz MIME type: {ContentType}", file.ContentType);
                return false;
            }

            return true;
        }

        // Generic görsel kaydetme metodu - tekrar eden kodları birleştirir
        private async Task<string?> SaveImageInternalAsync(
            IFormFile file, 
            string baseFolder, 
            bool isSecure, 
            bool convertToWebP = true,
            string logName = "Görsel")
        {
            if (!ValidateImage(file))
            {
                return null;
            }

            try
            {
                // Klasör yolu belirle
                var uploadsFolder = isSecure
                    ? Path.Combine(_environment.ContentRootPath, "Data", "uploads", baseFolder)
                    : Path.Combine(_environment.WebRootPath, "uploads", baseFolder);
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // WebP'ye dönüştür (opsiyonel)
                if (convertToWebP)
                {
                    var webPPath = await ResizeAndConvertToWebPAsync(filePath);
                    if (webPPath != null && webPPath != filePath)
                    {
                        // Orijinal dosyayı sil
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        fileName = Path.GetFileName(webPPath);
                    }
                }

                // Return value belirle
                if (isSecure)
                {
                    _logger.LogInformation("{LogName} kaydedildi: {FileName} (klasör: {Folder})", logName, fileName, baseFolder);
                    return fileName; // Sadece dosya adını döndür
                }
                else
                {
                    var relativePath = $"/uploads/{baseFolder}/{fileName}";
                    _logger.LogInformation("{LogName} kaydedildi: {Path}", logName, relativePath);
                    return relativePath;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{LogName} kaydedilirken hata oluştu", logName);
                return null;
            }
        }

        public async Task<string?> SaveImageAsync(IFormFile file, string folder, bool convertToWebP = true)
        {
            return await SaveImageInternalAsync(file, folder, isSecure: false, convertToWebP, "Görsel");
        }

        // Video dosyalarını güvenli private klasöre kaydeder
        // Sadece dosya adını döndürür (path değil)
        public async Task<string?> SaveSecureImageAsync(IFormFile file, string folder, bool convertToWebP = true)
        {
            return await SaveImageInternalAsync(file, folder, isSecure: true, convertToWebP, "Güvenli görsel");
        }

        // İzin verilen video uzantıları
        private readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg" };
        // Maksimum video boyutu (50 MB)
        private readonly long _maxVideoSize = 50 * 1024 * 1024;

        public bool ValidateVideo(IFormFile file, out string? errorMessage)
        {
            errorMessage = null;
            if (file == null || file.Length == 0)
            {
                errorMessage = "Dosya boş.";
                return false;
            }

            if (file.Length > _maxVideoSize)
            {
                errorMessage = $"Video boyutu en fazla {_maxVideoSize / (1024 * 1024)}MB olabilir.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedVideoExtensions.Contains(extension))
            {
                errorMessage = "Sadece .mp4, .webm ve .ogg formatları desteklenmektedir.";
                return false;
            }

            return true;
        }

        public async Task<string?> SaveVideoAsync(IFormFile file, string folder)
        {
            if (!ValidateVideo(file, out var error))
            {
                _logger.LogWarning("Video validasyonu başarısız: {Error}", error);
                return null;
            }

            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/{folder}/{fileName}";
                _logger.LogInformation("Video kaydedildi: {Path}", relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Video kaydedilirken hata oluştu");
                return null;
            }
        }

        // Private klasörden resmi okur (sadece dosya adı verilir)
        // Alternatif uzantıları da kontrol eder (jpg→webp, png→webp)
        public string? GetSecureImagePath(string fileName, string folder)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            try
            {
                var privateFolder = Path.Combine(_environment.ContentRootPath, "Data", "uploads", folder);
                
                // 1. Tam eşleşmeyi kontrol et
                var filePath = Path.Combine(privateFolder, fileName);
                if (File.Exists(filePath))
                {
                    _logger.LogDebug("Resim bulundu (tam eşleşme): {FilePath}", filePath);
                    return filePath;
                }
                
                // 2. Alternatif uzantıları kontrol et
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var currentExt = Path.GetExtension(fileName).ToLowerInvariant();
                
                // Eğer mevcut uzantı .jpg, .jpeg veya .png ise, .webp versiyonunu kontrol et
                if (currentExt == ".jpg" || currentExt == ".jpeg" || currentExt == ".png")
                {
                    var webPPath = Path.Combine(privateFolder, fileNameWithoutExt + ".webp");
                    if (File.Exists(webPPath))
                    {
                        _logger.LogDebug("Resim bulundu (WebP alternatifi): {WebPPath}", webPPath);
                        return webPPath;
                    }
                }
                // Eğer mevcut uzantı .webp ise, orijinal uzantıları kontrol et
                else if (currentExt == ".webp")
                {
                    var extensionsToCheck = new[] { ".jpg", ".jpeg", ".png" };
                    foreach (var ext in extensionsToCheck)
                    {
                        var altPath = Path.Combine(privateFolder, fileNameWithoutExt + ext);
                        if (File.Exists(altPath))
                        {
                            _logger.LogDebug("Resim bulundu ({Ext} alternatifi): {Path}", ext, altPath);
                            return altPath;
                        }
                    }
                }
                
                _logger.LogWarning("Resim bulunamadı (tüm alternatifler denendi): Folder={Folder}, FileName={FileName}", folder, fileName);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Güvenli görsel path'i alınırken hata oluştu: {FileName}", fileName);
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return false;
            }

            try
            {
                var fullPath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("Görsel silindi: {Path}", imagePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görsel silinirken hata oluştu: {Path}", imagePath);
                return false;
            }
        }

        // Görseli yeniden boyutlandırır ve WebP formatına dönüştürür
        // Varsayılan boyut: 800x600 (mobil performans için optimize edildi)
        public async Task<string?> ResizeAndConvertToWebPAsync(string imagePath, int maxWidth = 1920, int maxHeight = 1080)
        {
            try
            {
                using var input = File.OpenRead(imagePath);
                using var inputStream = new SKManagedStream(input);
                using var original = SKBitmap.Decode(inputStream);

                if (original == null)
                {
                    _logger.LogWarning("Görsel decode edilemedi: {Path}", imagePath);
                    return imagePath;
                }

                // En boy oranını koruyarak yeni boyutları hesapla
                var ratioX = (double)maxWidth / original.Width;
                var ratioY = (double)maxHeight / original.Height;
                var ratio = Math.Min(ratioX, ratioY);

                if (ratio >= 1)
                {
                    // Görsel zaten istenen boyuttan küçükse, sadece WebP'ye dönüştür
                    ratio = 1;
                }

                var newWidth = (int)(original.Width * ratio);
                var newHeight = (int)(original.Height * ratio);

                // Yeniden boyutlandırma işlemi (High Quality)
                using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
                if (resized == null)
                {
                    _logger.LogWarning("Görsel yeniden boyutlandırılamadı: {Path}", imagePath);
                    return imagePath;
                }

                // WebP formatında encode et (Kalite: 85 - Premium görüntü & Performans dengesi)
                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Webp, 85);

                var webPPath = Path.ChangeExtension(imagePath, ".webp");
                using (var output = File.OpenWrite(webPPath))
                {
                    data.SaveTo(output);
                }

                _logger.LogInformation("Görsel WebP'ye dönüştürüldü: {Path}", webPPath);
                return await Task.FromResult(webPPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Görsel dönüştürülürken hata oluştu: {Path}", imagePath);
                return imagePath;
            }
        }

        // Generic görsel validasyon metodu - özel format ve boyut kontrolleri için
        private bool ValidateImageWithRules(IFormFile file, string[]? allowedExtensions, long? maxSizeBytes, out string? errorMessage)
        {
            errorMessage = null;
            
            if (!ValidateImage(file))
            {
                errorMessage = "Geçersiz görsel dosyası.";
                return false;
            }

            // Extension kontrolü
            if (allowedExtensions != null && allowedExtensions.Length > 0)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                {
                    errorMessage = $"Geçerli formatlar: {string.Join(", ", allowedExtensions)}";
                    return false;
                }
            }

            // Boyut kontrolü
            if (maxSizeBytes.HasValue && file.Length > maxSizeBytes.Value)
            {
                var maxSizeMB = maxSizeBytes.Value / (1024.0 * 1024.0);
                errorMessage = $"Dosya boyutu en fazla {maxSizeMB:F1}MB olabilir.";
                return false;
            }

            return true;
        }

        public bool ValidateLogo(IFormFile file, out string? errorMessage)
        {
            return ValidateImageWithRules(file, 
                new[] { ".png", ".svg", ".jpg", ".jpeg" }, 
                2 * 1024 * 1024, 
                out errorMessage);
        }

        public bool ValidateFavicon(IFormFile file, out string? errorMessage)
        {
            return ValidateImageWithRules(file, 
                new[] { ".png", ".ico" }, 
                500 * 1024, 
                out errorMessage);
        }

        public bool ValidateOgImage(IFormFile file, out string? errorMessage)
        {
            return ValidateImageWithRules(file, 
                null, // Tüm geçerli formatlar
                5 * 1024 * 1024, 
                out errorMessage);
        }

        // Özel görsel tipleri için generic kaydetme metodu (Logo, Favicon, OG Image vb.)
        // Not: Validasyon çağıran metodda yapılmalıdır
        private async Task<string?> SaveSpecialImageAsync(
            IFormFile file,
            string folder,
            int? resizeWidth = null,
            int? resizeHeight = null,
            string? fixedFileName = null,
            string logName = "Görsel")
        {

            // Eğer resize gerekmiyorsa normal SaveImageAsync kullan
            if (resizeWidth == null && resizeHeight == null && fixedFileName == null)
            {
                return await SaveImageAsync(file, folder);
            }

            // Özel işleme gerekiyorsa (resize veya fixed filename)
            try
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", folder);
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Geçici dosyaya kaydet
                var tempFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
                var tempFilePath = Path.Combine(Path.GetTempPath(), tempFileName);

                using (var stream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Resize gerekiyorsa yap
                string? resizedPath = null;
                if (resizeWidth.HasValue && resizeHeight.HasValue)
                {
                    resizedPath = await ResizeAndConvertToWebPAsync(tempFilePath, resizeWidth.Value, resizeHeight.Value);
                }

                // Final dosya adını belirle
                var finalFileName = fixedFileName ?? 
                    (resizedPath != null && File.Exists(resizedPath) 
                        ? Path.GetFileName(resizedPath) 
                        : $"{Guid.NewGuid()}.webp");
                var finalFilePath = Path.Combine(uploadsFolder, finalFileName);
                
                if (resizedPath != null && File.Exists(resizedPath))
                {
                    File.Copy(resizedPath, finalFilePath, true);
                    File.Delete(resizedPath);
                }
                else
                {
                    // Boyutlandırma başarısız olursa orijinali kopyala
                    File.Copy(tempFilePath, finalFilePath, true);
                }

                // Geçici dosyayı temizle
                if (File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }

                var relativePath = $"/uploads/{folder}/{finalFileName}";
                _logger.LogInformation("{LogName} kaydedildi: {Path}", logName, relativePath);
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{LogName} kaydedilirken hata oluştu", logName);
                return null;
            }
        }

        public async Task<string?> SaveLogoAsync(IFormFile file)
        {
            if (!ValidateLogo(file, out var error))
            {
                _logger.LogWarning("Logo validasyonu başarısız: {Error}", error);
                return null;
            }
            return await SaveSpecialImageAsync(file, "site", logName: "Logo");
        }

        public async Task<string?> SaveFaviconAsync(IFormFile file)
        {
            if (!ValidateFavicon(file, out var error))
            {
                _logger.LogWarning("Favicon validasyonu başarısız: {Error}", error);
                return null;
            }
            return await SaveSpecialImageAsync(file, "site", resizeWidth: 48, resizeHeight: 48, fixedFileName: "favicon.webp", logName: "Favicon");
        }

        public async Task<string?> SaveOgImageAsync(IFormFile file)
        {
            if (!ValidateOgImage(file, out var error))
            {
                _logger.LogWarning("OG Image validasyonu başarısız: {Error}", error);
                return null;
            }
            return await SaveSpecialImageAsync(file, "site", resizeWidth: 1200, resizeHeight: 630, logName: "OG Image");
        }

        // Görsel path'ini CDN URL ile birleştirir (varsa)
        public string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                return string.Empty;
            }

            // Zaten tam URL ise (http:// veya https:// ile başlıyorsa) olduğu gibi döndür
            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // CDN URL varsa kullan
            var cdnUrl = _configuration["ImageSettings:CdnUrl"];
            if (!string.IsNullOrEmpty(cdnUrl))
            {
                // CDN URL'inin sonunda / olmamalı, path'in başında / olmalı
                cdnUrl = cdnUrl.TrimEnd('/');
                var cleanPath = imagePath.TrimStart('/');
                return $"{cdnUrl}/{cleanPath}";
            }

            // CDN yoksa normal path döndür
            return imagePath;
        }



    }
}

