using Ganss.Xss;

namespace boya_usta_web.Helpers
{
    // HTML içeriklerini güvenli hale getirmek için sanitization helper
    // XSS saldırılarına karşı koruma sağlar
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer;

        static HtmlSanitizerHelper()
        {
            _sanitizer = new HtmlSanitizer();
            
            // İzin verilen HTML tag'leri (admin içerikleri için gerekli)
            _sanitizer.AllowedTags.Add("p");
            _sanitizer.AllowedTags.Add("br");
            _sanitizer.AllowedTags.Add("strong");
            _sanitizer.AllowedTags.Add("b");
            _sanitizer.AllowedTags.Add("em");
            _sanitizer.AllowedTags.Add("i");
            _sanitizer.AllowedTags.Add("u");
            _sanitizer.AllowedTags.Add("ul");
            _sanitizer.AllowedTags.Add("ol");
            _sanitizer.AllowedTags.Add("li");
            _sanitizer.AllowedTags.Add("a");
            _sanitizer.AllowedTags.Add("span");
            _sanitizer.AllowedTags.Add("div");
            _sanitizer.AllowedTags.Add("h1");
            _sanitizer.AllowedTags.Add("h2");
            _sanitizer.AllowedTags.Add("h3");
            _sanitizer.AllowedTags.Add("h4");
            _sanitizer.AllowedTags.Add("h5");
            _sanitizer.AllowedTags.Add("h6");
            _sanitizer.AllowedTags.Add("iframe"); // Google Maps embed için
            
            // İzin verilen HTML attribute'ları
            _sanitizer.AllowedAttributes.Add("href");
            _sanitizer.AllowedAttributes.Add("target");
            _sanitizer.AllowedAttributes.Add("rel");
            _sanitizer.AllowedAttributes.Add("class");
            _sanitizer.AllowedAttributes.Add("style");
            _sanitizer.AllowedAttributes.Add("src"); // iframe için
            _sanitizer.AllowedAttributes.Add("width");
            _sanitizer.AllowedAttributes.Add("height");
            _sanitizer.AllowedAttributes.Add("frameborder");
            _sanitizer.AllowedAttributes.Add("allowfullscreen");
            _sanitizer.AllowedAttributes.Add("loading");
            _sanitizer.AllowedAttributes.Add("referrerpolicy");
            
            // Güvenli URL protokolleri
            _sanitizer.AllowedSchemes.Add("http");
            _sanitizer.AllowedSchemes.Add("https");
            _sanitizer.AllowedSchemes.Add("mailto");
            
            // JavaScript ve event handler'ları engelle
            _sanitizer.AllowedAttributes.Remove("onclick");
            _sanitizer.AllowedAttributes.Remove("onerror");
            _sanitizer.AllowedAttributes.Remove("onload");
        }

        // HTML içeriğini güvenli hale getirir (XSS koruması)
        public static string Sanitize(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            return _sanitizer.Sanitize(html);
        }

        // HTML içeriğini güvenli hale getirir ve yeni satırları <br/> ile değiştirir
        public static string SanitizeWithLineBreaks(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Önce yeni satırları <br/> ile değiştir
            var withBreaks = html.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "<br/>");
            
            // Sonra sanitize et
            return _sanitizer.Sanitize(withBreaks);
        }
    }
}

