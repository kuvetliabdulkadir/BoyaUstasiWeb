using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Xml.Linq;
using boya_usta_web.Helpers;
using boya_usta_web.Services;

namespace boya_usta_web.Controllers
{
    // SEO için dinamik endpoint'ler sağlar (sitemap.xml, robots.txt, llms.txt)
    public class SeoController : Controller
    {
        private readonly ISeoService _seoService;
        private readonly ISettingsService _settingsService;
        private readonly IProjectService _projectService;
        private readonly IServicePageService _servicePageService;

        public SeoController(
            ISeoService seoService,
            ISettingsService settingsService,
            IProjectService projectService,
            IServicePageService servicePageService)
        {
            _seoService = seoService;
            _settingsService = settingsService;
            _projectService = projectService;
            _servicePageService = servicePageService;
        }

        // Dinamik XML Sitemap oluşturur
        [Route("/sitemap.xml")]
        [ResponseCache(Duration = 3600)] // 1 saat cache
        public async Task<IActionResult> Sitemap()
        {
            var siteUrl = _seoService.GetSiteUrl();
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var urls = new List<XElement>();

            // Statik sayfalar
            urls.Add(CreateUrlElement(ns, siteUrl, "/", "weekly", "1.0"));
            urls.Add(CreateUrlElement(ns, siteUrl, "/hizmetler", "weekly", "0.9"));
            urls.Add(CreateUrlElement(ns, siteUrl, "/projeler", "weekly", "0.8"));
            urls.Add(CreateUrlElement(ns, siteUrl, "/simulator", "monthly", "0.7"));
            urls.Add(CreateUrlElement(ns, siteUrl, "/iletisim", "monthly", "0.6"));
            urls.Add(CreateUrlElement(ns, siteUrl, "/hakkimizda", "monthly", "0.5"));

            // Dinamik: Hizmetler
            var services = await _servicePageService.GetAllActiveServicesAsync();

            foreach (var service in services)
            {
                // ÖNEMLİ: Veritabanındaki Slug'ı kullanıyoruz. Yoksa Title'dan üretiyoruz.
                var slug = !string.IsNullOrEmpty(service.Slug) ? service.Slug : service.Title.ToSlug();
                var lastMod = service.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                
                urls.Add(CreateUrlElement(ns, siteUrl, $"/hizmetler/{slug}", "weekly", "0.8", lastMod));
            }

            // Dinamik: Projeler
            var projects = await _projectService.GetAllActiveProjectsAsync();

            foreach (var project in projects)
            {
                var slug = !string.IsNullOrEmpty(project.Slug) ? project.Slug : project.Title.ToSlug();
                var lastMod = project.UpdatedAt?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd");
                
                urls.Add(CreateUrlElement(ns, siteUrl, $"/projeler/{slug}", "monthly", "0.7", lastMod));
            }

            var xml = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(ns + "urlset", urls)
            );

            return Content(xml.ToString(), "application/xml", Encoding.UTF8);
        }

        // Dinamik robots.txt oluşturur
        [Route("/robots.txt")]
        [ResponseCache(Duration = 86400)] // 24 saat cache
        public IActionResult Robots()
        {
            var siteUrl = _seoService.GetSiteUrl();

            var siteName = _seoService.GetSiteName();
            var content = $@"# {(string.IsNullOrEmpty(siteName) ? "Site" : siteName)} - robots.txt
# Oluşturulma: {DateTime.Now:yyyy-MM-dd}

User-agent: *
Allow: /
Disallow: /usta-panel-2024/
Disallow: /hata/
Disallow: /Error/
Disallow: /Account/

# Sitemap konumu
Sitemap: {siteUrl}/sitemap.xml

# AI Botları
User-agent: GPTBot
Allow: /

User-agent: Google-Extended
Allow: /

User-agent: ChatGPT-User
Allow: /

User-agent: anthropic-ai
Allow: /

# Crawl-delay (opsiyonel)
# Crawl-delay: 1
";
            return Content(content, "text/plain", Encoding.UTF8);
        }

        // AI botları için llms.txt oluşturur
        [Route("/llms.txt")]
        [ResponseCache(Duration = 86400)] // 24 saat cache
        public async Task<IActionResult> LlmsTxt()
        {
            var settings = await _settingsService.GetAllSettingsAsync();
            var siteName = settings.GetValueOrDefault("SiteName", "");
            var experienceYears = settings.GetValueOrDefault("ExperienceYears", "5");

            // Aktif hizmetleri al
            var services = await _servicePageService.GetAllActiveServicesAsync();
            var serviceTitles = services.Select(s => s.Title).ToList();

            var serviceList = serviceTitles.Any()
                ? string.Join("\n", serviceTitles.Select(s => $"- {s}"))
                : "- İç Cephe Boya\n- Dış Cephe Boya\n- Dekoratif Boya";

            var geoPlacename = settings.GetValueOrDefault("SeoGeoPlacename", "");
            var location = !string.IsNullOrEmpty(geoPlacename) ? geoPlacename : "Türkiye";
            
            var content = $@"# {(string.IsNullOrEmpty(siteName) ? "Site" : siteName)}

> {(!string.IsNullOrEmpty(experienceYears) ? $"{experienceYears} yıllık tecrübeyle " : "")}Profesyonel hizmetler sunan firma.

## Hakkımızda
{(string.IsNullOrEmpty(siteName) ? "Firmamız" : siteName)}, kaliteli hizmetler sunmaktadır.

## Hizmetlerimiz
{serviceList}

## Özelliklerimiz
- Ücretsiz keşif ve fiyat teklifi
- Garanti
- Kaliteli malzeme kullanımı
- Profesyonel ekip
- Zamanında teslimat

## Hizmet Bölgesi
- Konum: {location}

## İletişim
- Web: {_seoService.GetSiteUrl()}

---
Son güncelleme: {DateTime.Now:yyyy-MM-dd}
";
            return Content(content, "text/plain", Encoding.UTF8);
        }

        // Sitemap için URL elementi oluşturur
        private static XElement CreateUrlElement(
            XNamespace ns,
            string siteUrl,
            string path,
            string changeFreq,
            string priority,
            string? lastMod = null)
        {
            var url = new XElement(ns + "url",
                new XElement(ns + "loc", $"{siteUrl}{path}"),
                new XElement(ns + "changefreq", changeFreq),
                new XElement(ns + "priority", priority)
            );

            if (!string.IsNullOrEmpty(lastMod))
            {
                url.Add(new XElement(ns + "lastmod", lastMod));
            }

            return url;
        }
    }
}
