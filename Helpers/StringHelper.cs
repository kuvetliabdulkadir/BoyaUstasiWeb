using System.Text.RegularExpressions;
using System.Text;

namespace boya_usta_web.Helpers
{
    // String manipülasyonları için yardımcı sınıf
    public static class StringHelper
    {
        // Türkçe karakter dönüşüm tablosu (Performans için static readonly)
        private static readonly Dictionary<char, char> TurkishCharMap = new()
        {
            { 'ı', 'i' }, { 'İ', 'i' }, { 'I', 'i' },
            { 'ğ', 'g' }, { 'Ğ', 'g' },
            { 'ü', 'u' }, { 'Ü', 'u' },
            { 'ş', 's' }, { 'Ş', 's' },
            { 'ö', 'o' }, { 'Ö', 'o' },
            { 'ç', 'c' }, { 'Ç', 'c' }
        };

        // Metni URL-dostu slug formatına çevirir (Örn: "Örnek Başlık" -> "ornek-baslik")
        public static string ToSlug(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // 1. Türkçe karakterleri ASCII karşılıklarına dönüştür
            var sb = new StringBuilder(text.Length);
            foreach (var c in text)
            {
                sb.Append(TurkishCharMap.TryGetValue(c, out var mapped) ? mapped : c);
            }

            // 2. Küçük harfe çevir (Invariant Culture kullanarak)
            var result = sb.ToString().ToLowerInvariant();

            // 3. Alfanumerik olmayan karakterleri boşlukla değiştir (tire hariç)
            result = Regex.Replace(result, @"[^a-z0-9\s-]", " ");

            // 4. Birden fazla boşluk veya tireyi tek tire yap
            result = Regex.Replace(result, @"[\s-]+", "-");

            // 5. Baş ve sondaki tireleri kaldır
            return result.Trim('-');
        }
    }
}
