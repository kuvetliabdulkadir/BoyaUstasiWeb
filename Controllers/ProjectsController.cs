using Microsoft.AspNetCore.Mvc;
using boya_usta_web.Services;
using boya_usta_web.Helpers;

namespace boya_usta_web.Controllers
{
    // Projeleri listeleme ve detaylarını görüntüleme kontrolcüsü.
    public class ProjectsController : BaseController
    {
        private readonly IProjectService _projectService;

        public ProjectsController(
            IProjectService projectService, 
            ISettingsService settingsService,
            IContactFormService contactFormService)
            : base(settingsService, contactFormService)
        {
            _projectService = projectService;
        }

        // Projeleri listele (Opsiyonel kategori filtresi ile)
        public async Task<IActionResult> Index(string? category = null)
        {
            var projects = await _projectService.GetAllActiveProjectsAsync(category);

            // Kategoriler
            ViewBag.Categories = await _projectService.GetProjectCategoriesAsync();

            ViewBag.SelectedCategory = category;

            // Ayarları yükle (BaseController'dan gelir)
            var settings = await LoadSettingsAsync();
            
            // Sayfa başlıklarını ayarlardan al
            ViewBag.ProjectsCtaTitle = settings.GetSetting("ProjectsCtaTitle", "Sizin projeniz de burada olabilir!");
            ViewBag.ProjectsCtaDescription = settings.GetSetting("ProjectsCtaDescription", "Ücretsiz keşif için iletişime geçin.");
            ViewBag.ProjectsPageHeaderSubtitle = settings.GetSetting("ProjectsPageHeaderSubtitle", "Tamamladığımız işlerden örnekler");

            return View(projects);
        }

        // Proje detaylarını görüntüle (ID ile)
        public async Task<IActionResult> Detail(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound();

            // Eğer slug varsa ve URL'de ID kullanılmışsa slug'lı URL'e yönlendir (SEO için)
            if (!string.IsNullOrEmpty(project.Slug))
                return RedirectToRoute("project-detail", new { slug = project.Slug });

            // Ayarları yükle (Header/Footer için gerekli)
            await LoadSettingsAsync();

            // Benzer projeler
            ViewBag.SimilarProjects = await _projectService.GetSimilarProjectsAsync(id, project.Category);

            return View(project);
        }

        // Proje detaylarını görüntüle (Slug/URL ile for SEO)
        public async Task<IActionResult> DetailBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
                return NotFound();

            var project = await _projectService.GetProjectBySlugAsync(slug);

            if (project == null)
                return NotFound();

            await LoadSettingsAsync();

            // Benzer projeler
            ViewBag.SimilarProjects = await _projectService.GetSimilarProjectsAsync(project.Id, project.Category);

            return View("Detail", project);
        }
    }
}

