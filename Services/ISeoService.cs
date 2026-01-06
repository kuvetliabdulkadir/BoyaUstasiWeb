namespace boya_usta_web.Services
{
    // SEO servis arayüzü - Sayfa bazlı SEO meta verilerini yönetir
    public interface ISeoService
    {

        // Sayfa başlığı (title tag)
        string PageTitle { get; }

        // Meta description
        string MetaDescription { get; }

        // Meta keywords
        string Keywords { get; }

        // Canonical URL
        string CanonicalUrl { get; }

        // Robots meta içeriği (index, follow vb.)
        string RobotsContent { get; }


        // Open Graph başlık
        string OgTitle { get; }

        // Open Graph açıklama
        string OgDescription { get; }

        // Open Graph görsel URL
        string OgImage { get; }

        // Open Graph sayfa URL
        string OgUrl { get; }

        // Open Graph içerik tipi (website, article vb.)
        string OgType { get; }

        // Open Graph dil kodu (tr_TR)
        string OgLocale { get; }

        // Open Graph site adı
        string OgSiteName { get; }


        // Twitter Card tipi (summary_large_image)
        string TwitterCard { get; }

        // Twitter Card başlık
        string TwitterTitle { get; }

        // Twitter Card açıklama
        string TwitterDescription { get; }

        // Twitter Card görsel
        string TwitterImage { get; }


        // Bölge kodu (ISO 3166-2)
        string GeoRegion { get; }

        // Şehir adı
        string GeoPlacename { get; }

        // Enlem koordinatı
        string GeoLatitude { get; }

        // Boylam koordinatı
        string GeoLongitude { get; }

        // Geo position formatı (enlem;boylam)
        string GeoPosition { get; }

        // ICBM formatı (enlem, boylam)
        string ICBM { get; }


        // Sayfa SEO meta verilerini ayarlar
        // title: Sayfa başlığı (null ise default kullanılır)
        // description: Meta açıklama (null ise default kullanılır)
        // keywords: Anahtar kelimeler (null ise default kullanılır)
        // robots: Robots içeriği (null ise "index, follow" kullanılır)
        // ogImage: Open Graph görseli (null ise default kullanılır)
        // ogType: Open Graph tipi (null ise "website" kullanılır)
        void SetPageMeta(
            string? title = null,
            string? description = null,
            string? keywords = null,
            string? robots = null,
            string? ogImage = null,
            string? ogType = null);

        // Canonical URL'i manuel olarak ayarlar
        void SetCanonicalUrl(string url);

        // Robots meta içeriğini ayarlar
        void SetRobots(string content);

        // Open Graph görselini ayarlar
        void SetOgImage(string imageUrl);

        // Yapılandırmadan site URL'ini döndürür
        string GetSiteUrl();

        // Site adını döndürür
        string GetSiteName();

        // Tema rengini döndürür
        string GetThemeColor();


    }
}

