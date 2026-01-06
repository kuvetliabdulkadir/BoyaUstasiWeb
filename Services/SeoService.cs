using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // SEO servis implementasyonu - Sayfa bazlı SEO meta verilerini yönetir
    // Öncelik sırası: Controller Set -> Sayfa SEO (PageSeoSettings) -> Genel SEO (SiteSettings) -> appsettings.json
    public class SeoService : ISeoService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingsService _settingsService;
        private readonly IPageSeoService _pageSeoService;
        
        // Veritabanı ayarları için önbellek (her istekte bir kez yüklenir)
        private Dictionary<string, string?>? _cachedDbSettings;
        private bool _dbSettingsLoaded = false;
        
        // Sayfa SEO ayarları için önbellek (her istekte bir kez yüklenir)
        private PageSeoSetting? _cachedPageSeo;
        private bool _pageSeoLoaded = false;

        // Geçici değerler (Controller'dan set edilenler)
        private string? _pageTitle;
        private string? _metaDescription;
        private string? _keywords;
        private string? _canonicalUrl;
        private string? _robotsContent;
        private string? _ogImage;
        private string? _ogType;

        public SeoService(
            IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor,
            ISettingsService settingsService,
            IPageSeoService pageSeoService)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _settingsService = settingsService;
            _pageSeoService = pageSeoService;
        }



        // Mevcut sayfa için SEO ayarlarını yükler (lazy loading)
        // Path normalizasyonu PageSeoService tarafından yapılır
        private void EnsurePageSeoLoaded()
        {
            if (_pageSeoLoaded)
            {
                return;
            }
            
            try
            {
                var path = _httpContextAccessor.HttpContext?.Request.Path.Value ?? "/";
                
                // Sync metot kullan - deadlock'u önler
                _cachedPageSeo = _pageSeoService.GetPageSeo(path);
                
                _pageSeoLoaded = true;
            }
            catch
            {
                _cachedPageSeo = null;
                _pageSeoLoaded = true;
            }
        }

        // Veritabanından tüm genel SEO ayarlarını yükler (lazy loading)
        private void EnsureDbSettingsLoaded()
        {
            if (_dbSettingsLoaded)
            {
                return;
            }
            
            try
            {
                // Sync metot kullan - deadlock'u önler
                _cachedDbSettings = _settingsService.GetAllSettings();
                _dbSettingsLoaded = true;
            }
            catch
            {
                _cachedDbSettings = new Dictionary<string, string?>();
                _dbSettingsLoaded = true;
            }
        }

        // ViewData'dan değer okur
        private string? GetViewDataValue(string key)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Items.TryGetValue($"SEO_{key}", out var value) == true)
            {
                return value?.ToString();
            }
            return null;
        }

        // Genel SEO ayarını okur (SiteSettings tablosundan)
        private string GetSeoSetting(string dbKey, string configKey, string defaultValue = "")
        {
            EnsureDbSettingsLoaded();
            
            if (_cachedDbSettings != null && 
                _cachedDbSettings.TryGetValue(dbKey, out var dbValue) && 
                !string.IsNullOrEmpty(dbValue))
            {
                return dbValue;
            }

            return _configuration[$"SeoSettings:{configKey}"] ?? defaultValue;
        }

        // Mevcut request URL'ini döndürür
        private string GetCurrentUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                return GetSiteUrl();
            }

            return $"{request.Scheme}://{request.Host}{request.Path}";
        }

        // Görsel URL'ini tam URL'e çevirir
        private string GetAbsoluteImageUrl(string? relativePath)
        {
            var defaultOgImage = GetSeoSetting("SeoDefaultOgImage", "DefaultOgImage", "/images/og-image.png");
            
            if (string.IsNullOrEmpty(relativePath))
            {
                return $"{GetSiteUrl()}{defaultOgImage}";
            }

            if (relativePath.StartsWith("http://") || relativePath.StartsWith("https://"))
            {
                return relativePath;
            }

            return $"{GetSiteUrl()}{relativePath}";
        }



        public string PageTitle
        {
            get
            {
                // 1. Controller'dan set edilen değer
                if (!string.IsNullOrEmpty(_pageTitle))
                {
                    return _pageTitle;
                }

                // 2. Sayfa bazlı SEO (PageSeoSettings)
                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.Title))
                {
                    return _cachedPageSeo.Title;
                }

                // 3. Genel SEO ayarları (SiteSettings)
                // Önce SeoDefaultTitle'a bak, yoksa SiteName'i kullan
                var title = GetSeoSetting("SeoDefaultTitle", "DefaultTitle", "");
                if (string.IsNullOrEmpty(title))
                {
                    title = GetSeoSetting("SiteName", "SiteName", "");
                }
                return title;
            }
        }

        public string MetaDescription
        {
            get
            {
                if (!string.IsNullOrEmpty(_metaDescription))
                {
                    return _metaDescription;
                }

                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.MetaDescription))
                {
                    return _cachedPageSeo.MetaDescription;
                }

                // Önce SeoDefaultDescription'a bak, yoksa MetaDescription'ı kullan
                var desc = GetSeoSetting("SeoDefaultDescription", "DefaultDescription", "");
                if (string.IsNullOrEmpty(desc))
                {
                    desc = GetSeoSetting("MetaDescription", "MetaDescription", "");
                }
                return desc;
            }
        }

        public string Keywords
        {
            get
            {
                if (!string.IsNullOrEmpty(_keywords))
                {
                    return _keywords;
                }

                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.Keywords))
                {
                    return _cachedPageSeo.Keywords;
                }

                return GetSeoSetting("SeoDefaultKeywords", "DefaultKeywords", "");
            }
        }

        public string CanonicalUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(_canonicalUrl))
                {
                    return _canonicalUrl;
                }

                return GetCurrentUrl();
            }
        }

        public string RobotsContent
        {
            get
            {
                if (!string.IsNullOrEmpty(_robotsContent))
                {
                    return _robotsContent;
                }

                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.Robots))
                {
                    return _cachedPageSeo.Robots;
                }

                return GetSeoSetting("SeoRobotsDefault", "RobotsDefault", "index, follow");
            }
        }



        public string OgTitle => PageTitle;

        public string OgDescription => MetaDescription;

        public string OgImage
        {
            get
            {
                if (!string.IsNullOrEmpty(_ogImage))
                {
                    return GetAbsoluteImageUrl(_ogImage);
                }

                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.OgImage))
                {
                    return GetAbsoluteImageUrl(_cachedPageSeo.OgImage);
                }

                return GetAbsoluteImageUrl(null);
            }
        }

        public string OgUrl => CanonicalUrl;

        public string OgType
        {
            get
            {
                if (!string.IsNullOrEmpty(_ogType))
                {
                    return _ogType;
                }

                EnsurePageSeoLoaded();
                if (_cachedPageSeo != null && !string.IsNullOrEmpty(_cachedPageSeo.OgType))
                {
                    return _cachedPageSeo.OgType;
                }

                return "website";
            }
        }

        public string OgLocale => GetSeoSetting("SeoLocale", "Locale", "tr_TR");

        public string OgSiteName 
        {
            get 
            {
                var siteName = GetSeoSetting("SeoSiteName", "SiteName", "");
                if (string.IsNullOrEmpty(siteName))
                {
                    siteName = GetSeoSetting("SiteName", "SiteName", "");
                }
                return siteName;
            }
        }



        public string TwitterCard => "summary_large_image";

        public string TwitterTitle => PageTitle;

        public string TwitterDescription => MetaDescription;

        public string TwitterImage => OgImage;



        public string GeoRegion => GetSeoSetting("SeoGeoRegion", "GeoRegion", "TR-16");

        public string GeoPlacename => GetSeoSetting("SeoGeoPlacename", "GeoPlacename", "");

        public string GeoLatitude => GetSeoSetting("SeoGeoLatitude", "GeoLatitude", "40.1885");

        public string GeoLongitude => GetSeoSetting("SeoGeoLongitude", "GeoLongitude", "29.0610");

        public string GeoPosition => $"{GeoLatitude};{GeoLongitude}";

        public string ICBM => $"{GeoLatitude}, {GeoLongitude}";



        public void SetPageMeta(
            string? title = null,
            string? description = null,
            string? keywords = null,
            string? robots = null,
            string? ogImage = null,
            string? ogType = null)
        {
            _pageTitle = title;
            _metaDescription = description;
            _keywords = keywords;
            _robotsContent = robots;
            _ogImage = ogImage;
            _ogType = ogType;
        }

        public void SetCanonicalUrl(string url)
        {
            _canonicalUrl = url;
        }

        public void SetRobots(string content)
        {
            _robotsContent = content;
        }

        public void SetOgImage(string imageUrl)
        {
            _ogImage = imageUrl;
        }

        public string GetSiteUrl()
        {
            var siteUrl = GetSeoSetting("SeoSiteUrl", "SiteUrl", "");
            
            if (string.IsNullOrEmpty(siteUrl))
            {
                var request = _httpContextAccessor.HttpContext?.Request;
                if (request != null)
                {
                    siteUrl = $"{request.Scheme}://{request.Host}";
                }
            }

            return siteUrl.TrimEnd('/');
        }

        public string GetSiteName()
        {
            return GetSeoSetting("SeoSiteName", "SiteName", "");
        }

        public string GetThemeColor()
        {
            return GetSeoSetting("SeoThemeColor", "ThemeColor", "#2c3e50");
        }


    }
}
