using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Helpers;

namespace boya_usta_web.Services
{
    // İletişim formu işleme, validasyon ve bildirim servisi
    public class ContactFormService : IContactFormService
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IEmailService _emailService;
        private readonly ISettingsService _settingsService;
        private readonly ITurnstileService _turnstileService;
        private readonly ILogger<ContactFormService> _logger;

        public ContactFormService(
            ApplicationDbContext context,
            IImageService imageService,
            IEmailService emailService,
            ISettingsService settingsService,
            ITurnstileService turnstileService,
            ILogger<ContactFormService> logger)
        {
            _context = context;
            _imageService = imageService;
            _emailService = emailService;
            _settingsService = settingsService;
            _turnstileService = turnstileService;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, string? WhatsAppLink)> ProcessContactFormAsync(ContactMessage model, IFormFile[]? attachments, string ipAddress, string userAgent, string? website)
        {
            try
            {
                // Sanitize Inputs
                model.FullName = SanitizationService.Sanitize(model.FullName);
                model.Phone = SanitizationService.Sanitize(model.Phone);
                model.Email = SanitizationService.Sanitize(model.Email);
                model.City = SanitizationService.Sanitize(model.City);
                model.District = SanitizationService.Sanitize(model.District);
                model.ServiceType = SanitizationService.Sanitize(model.ServiceType);
                model.Message = SanitizationService.Sanitize(model.Message);

                // 1. Validasyonlar
                var validationResult = ValidateForm(model, website);
                if (!validationResult.IsValid)
                {
                    return (false, validationResult.ErrorMessage, null);
                }

                // 2. Fotoğraf Yükleme
                if (attachments != null && attachments.Length > 0)
                {
                    if (attachments.Length > 5)
                    {
                        return (false, "Maksimum 5 adet resim yükleyebilirsiniz.", null);
                    }

                    var validAttachments = attachments
                        .Where(a => a != null && _imageService.ValidateImage(a))
                        .Take(5)
                        .ToList();

                    if (validAttachments.Any())
                    {
                        var attachmentFileNames = new List<string>();
                        foreach (var attachment in validAttachments)
                        {
                            var filePath = await _imageService.SaveImageAsync(attachment, "messages", convertToWebP: true);
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                attachmentFileNames.Add(filePath);
                            }
                        }

                        if (attachmentFileNames.Any())
                        {
                            model.AttachmentPaths = JsonSerializer.Serialize(attachmentFileNames);
                        }
                    }
                }

                // 3. Veritabanı Kayıt
                model.IpAddress = ipAddress;
                model.UserAgent = userAgent;

                _context.ContactMessages.Add(model);
                await _context.SaveChangesAsync();

                // 4. Rate Limiting Kayıt
                _turnstileService.RecordFormSubmission(ipAddress);

                // 5. E-posta Gönderimi
                await _emailService.SendContactNotificationAsync(
                    model.FullName ?? "Bilinmiyor",
                    model.Phone ?? "Bilinmiyor",
                    model.Message ?? "Açıklama girilmemiş",
                    attachments
                );

                _logger.LogInformation("Yeni iletişim formu: {Name}, {Phone}", model.FullName, model.Phone);

                // 6. WhatsApp Link Oluşturma
                string? whatsappLink = null;
                var settings = await _settingsService.GetAllSettingsAsync();
                var whatsappNumber = settings.TryGetValue("WhatsApp", out var val) ? val : null;

                if (!string.IsNullOrEmpty(whatsappNumber))
                {
                    var whatsappMessage = FormatWhatsAppMessage(model);
                    whatsappLink = CreateWhatsAppLink(whatsappNumber, whatsappMessage);
                }

                return (true, "Mesajınız başarıyla gönderildi.", whatsappLink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İletişim formu işlenirken hata oluştu");
                return (false, "Bir hata oluştu. Lütfen tekrar deneyin.", null);
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateForm(ContactMessage model, string? website)
        {
            // Honeypot
            if (!string.IsNullOrEmpty(website) && !string.IsNullOrWhiteSpace(website))
            {
                return (false, "Spam tespit edildi.");
            }

            // XSS Sanitize
            if (!string.IsNullOrEmpty(model.District))
            {
                var safeDistrict = System.Text.RegularExpressions.Regex.Replace(
                    model.District, @"[^a-zA-ZğüşıöçĞÜŞİÖÇ\s]", string.Empty);
                model.District = safeDistrict;
            }

            // Telefon Validasyonu
            if (!string.IsNullOrEmpty(model.Phone))
            {
                var phoneDigits = new string(model.Phone.Where(char.IsDigit).ToArray());
                if (phoneDigits.Length < 7 || phoneDigits.Length > 15 || phoneDigits.Length == 0)
                {
                    return (false, "Geçersiz telefon numarası.");
                }
            }

            // Metrekare Validasyonu
            if (model.MinSquareMeters.HasValue && model.MaxSquareMeters.HasValue)
            {
                if (model.MinSquareMeters.Value > model.MaxSquareMeters.Value)
                {
                    return (false, "Maksimum metrekare, minimum metrekareden küçük olamaz.");
                }
            }

            return (true, string.Empty);
        }

        private string FormatWhatsAppMessage(ContactMessage model)
        {
            var message = new StringBuilder();
            message.AppendLine("*Yeni İletişim Formu*\n");
            message.AppendLine($"*Ad Soyad:* {model.FullName ?? "-"}");
            message.AppendLine($"*Telefon:* {model.Phone ?? "-"}");
            if (!string.IsNullOrEmpty(model.Email)) message.AppendLine($"*E-posta:* {model.Email}");
            message.AppendLine($"*İl:* {model.City ?? "-"}");
            message.AppendLine($"*İlçe:* {model.District ?? "-"}");
            message.AppendLine($"*Hizmet:* {model.ServiceType ?? "-"}");

            if (model.MinSquareMeters.HasValue && model.MaxSquareMeters.HasValue)
                message.AppendLine($"*Metrekare:* {model.MinSquareMeters}-{model.MaxSquareMeters} m²");
            else if (model.SquareMeters.HasValue)
                message.AppendLine($"*Metrekare:* {model.SquareMeters} m²");

            if (!string.IsNullOrEmpty(model.Message))
                message.AppendLine($"\n*Mesaj:*\n{model.Message}");

            if (!string.IsNullOrEmpty(model.AttachmentPaths))
                message.AppendLine($"\n*Fotoğraf:* Eklendi");

            return message.ToString();
        }

        private string CreateWhatsAppLink(string phoneNumber, string message)
        {
            var cleanPhone = phoneNumber
                .Replace(" ", "").Replace("+", "").Replace("(", "")
                .Replace(")", "").Replace("-", "").Trim();
            var encodedMessage = Uri.EscapeDataString(message);
            return $"https://wa.me/{cleanPhone}?text={encodedMessage}";
        }

        public async Task<List<string>> GetActiveServiceTitlesAsync()
        {
            var activeServices = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Title)
                .Select(s => s.Title)
                .ToListAsync();

            return activeServices.Any()
                ? activeServices
                : new List<string> { "İç Mekan Boyama", "Dış Cephe Boyama", "Dekoratif Uygulama", "Duvar Kağıdı", "Diğer" };
        }

        public (List<string> Cities, Dictionary<string, List<string>> CityDistrictsMap, string DefaultCity) GetCityData(Dictionary<string, string?> settings)
        {
            var cities = boya_usta_web.Helpers.CityDistrictsHelper.GetCities(settings);
            var cityDistrictsMap = boya_usta_web.Helpers.CityDistrictsHelper.GetCityDistrictsMap(settings, cities);
            var defaultCity = boya_usta_web.Helpers.CityDistrictsHelper.GetDefaultCity(cities);

            return (cities, cityDistrictsMap, defaultCity);
        }
    }
}
