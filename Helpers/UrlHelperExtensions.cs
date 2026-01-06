using System.Text;
using System.Text.RegularExpressions;

namespace boya_usta_web.Helpers
{
    // URL ve slug oluşturma yardımcı metodları
    public static class UrlHelperExtensions
    {
        // WhatsApp linki oluşturur (varsayılan mesaj ile)
        public static string GetWhatsAppLink(string phoneNumber, string? message = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return string.Empty;

            // Telefon numarasını temizle (boşluk, +, - karakterlerini kaldır)
            var cleanPhone = phoneNumber.Replace(" ", "").Replace("+", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Mesaj varsa URL encode et
            if (!string.IsNullOrWhiteSpace(message))
            {
                var encodedMessage = Uri.EscapeDataString(message);
                return $"https://wa.me/{cleanPhone}?text={encodedMessage}";
            }

            return $"https://wa.me/{cleanPhone}";
        }


    }
}

