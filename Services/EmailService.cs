using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace boya_usta_web.Services
{
    // E-posta ayarları modeli (appsettings.json'dan okunur)
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int Port { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string RecipientEmails { get; set; } = string.Empty; // Virgülle ayrılmış alıcı e-postaları
    }

    // SMTP üzerinden e-posta gönderim servisi
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            // appsettings.json dosyasından EmailSettings bölümünü oku
            _settings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
            _logger = logger;
        }

        // E-posta gönderme işlemi (Asenkron) - Tek alıcı
        public async Task<bool> SendEmailAsync(string to, string subject, string body, IFormFile[]? attachments = null)
        {
            try
            {
                // SMTP ayarları kontrolü
                if (string.IsNullOrEmpty(_settings.SmtpServer) || string.IsNullOrEmpty(_settings.Username))
                {
                    _logger.LogWarning("E-posta ayarları yapılandırılmamış.");
                    return false;
                }

                // SMTP istemcisini oluştur
                using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                    EnableSsl = true
                };

                // E-posta mesajını hazırla
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                // Varsa ekleri (attachments) ekle
                if (attachments != null && attachments.Length > 0)
                {
                    foreach (var file in attachments)
                    {
                        if (file.Length > 0)
                        {
                            var attachment = new Attachment(file.OpenReadStream(), file.FileName, file.ContentType);
                            mailMessage.Attachments.Add(attachment);
                        }
                    }
                }

                // E-postayı gönder
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("E-posta başarıyla gönderildi: {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {To}", to);
                return false;
            }
        }

        // E-posta gönderme overload (Eksiz)
        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            return await SendEmailAsync(to, subject, body, null);
        }

        // İletişim formu bildirimi gönderme - Birden fazla alıcıya gönderir
        public async Task<bool> SendContactNotificationAsync(string customerName, string phone, string message, IFormFile[]? attachments = null)
        {
            var subject = $"Yeni İletişim Formu: {customerName}";
            var body = $@"
                <h2>Yeni İletişim Formu Mesajı</h2>
                <p><strong>Ad Soyad:</strong> {customerName}</p>
                <p><strong>Telefon:</strong> {phone}</p>
                <p><strong>Mesaj:</strong></p>
                <p>{message}</p>
                <hr>
                <p><small>Bu mesaj web sitenizden gönderilmiştir.</small></p>
            ";

            // Alıcı e-postaları al (RecipientEmails varsa onu kullan, yoksa SenderEmail'e gönder)
            var recipients = GetRecipientEmails();
            
            bool allSuccess = true;
            foreach (var recipient in recipients)
            {
                var result = await SendEmailAsync(recipient, subject, body, attachments);
                if (!result) allSuccess = false;
            }
            
            return allSuccess;
        }
        
        // Alıcı e-postalarını döndürür
        private List<string> GetRecipientEmails()
        {
            var recipients = new List<string>();
            
            // RecipientEmails varsa virgülle ayırarak al
            if (!string.IsNullOrEmpty(_settings.RecipientEmails))
            {
                var emails = _settings.RecipientEmails.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var email in emails)
                {
                    var trimmed = email.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        recipients.Add(trimmed);
                    }
                }
            }
            
            // Eğer hiç alıcı yoksa, SenderEmail'i kullan
            if (recipients.Count == 0 && !string.IsNullOrEmpty(_settings.SenderEmail))
            {
                recipients.Add(_settings.SenderEmail);
            }
            
            return recipients;
        }
    }
}

