namespace boya_usta_web.Services
{
    // E-posta gönderim servis arayüzü
    public interface IEmailService
    {
        // Basit e-posta gönderimi
        Task<bool> SendEmailAsync(string to, string subject, string body);
        
        // Ekli dosya ile e-posta gönderimi
        Task<bool> SendEmailAsync(string to, string subject, string body, IFormFile[]? attachments = null);

        // İletişim formu bildirimi
        Task<bool> SendContactNotificationAsync(string customerName, string phone, string message, IFormFile[]? attachments = null);
    }
}

