using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Sentry Entegrasyonu (Hata İzleme)
builder.WebHost.UseSentry(o =>
{
    // DSN appsettings'den okunacak
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    
    // Debug modunda daha fazla bilgi
    o.Debug = builder.Environment.IsDevelopment();
    
    // Performans izleme oranı (Production'da %20)
    o.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
    
    // Ortam bilgisi
    o.Environment = builder.Environment.EnvironmentName;
    
    // Release versiyon
    o.Release = "boya-usta-web@1.2.0";
    
    // Profil bilgilerini gönder (Performans izleme için)
    o.ProfilesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.1;
    
    // Kullanıcı bilgilerini otomatik ekle
    o.SendDefaultPii = false; // KVKK uyumluluğu için kapalı
    
    // Breadcrumbs - Son işlemleri kaydet
    o.MaxBreadcrumbs = 50;
    
    // Attachment boyut limiti
    o.MaxAttachmentSize = 10 * 1024 * 1024; // 10MB
});

// DbContext ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity ekle
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre gereksinimleri
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    // Kilitleme ayarları
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;

    // Kullanıcı ayarları
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Data Protection - Anahtarları dosyada sakla
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Data", "keys")))
    .SetApplicationName("BoyaUstaWeb");

// Cookie ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.LoginPath = "/usta-panel-2024/giris";
    options.LogoutPath = "/usta-panel-2024/cikis";
    options.AccessDeniedPath = "/Error/403";
});

// Anti-forgery
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
});

// Servisleri ekle
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IGoogleReviewsService, GoogleReviewsService>();
builder.Services.AddScoped<IPageSeoService, PageSeoService>();
builder.Services.AddScoped<ISeoService, SeoService>();
builder.Services.AddHttpClient<IGoogleReviewsService, GoogleReviewsService>();
builder.Services.AddHttpClient<ITurnstileService, TurnstileService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IContactFormService, ContactFormService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IServicePageService, ServicePageService>();
builder.Services.AddScoped<ISimulatorService, SimulatorService>();

// Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
});

// MVC
builder.Services.AddControllersWithViews(options =>
{
    // Request body size limitini 60MB'a çıkar (3D modeller için)
    options.MaxModelBindingCollectionSize = int.MaxValue;
});

// Form options - Büyük dosya yükleme için
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 60 * 1024 * 1024; // 60 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
    options.MultipartBoundaryLengthLimit = int.MaxValue;
    options.KeyLengthLimit = int.MaxValue;
});

var app = builder.Build();

// Veritabanını oluştur ve seed data ekle
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
        
        // Boş Slug değerlerini düzelt
        var db = services.GetRequiredService<ApplicationDbContext>();
        var servicesWithEmptySlug = db.Services.Where(s => string.IsNullOrEmpty(s.Slug)).ToList();
        foreach (var s in servicesWithEmptySlug)
        {
            s.Slug = boya_usta_web.Helpers.StringHelper.ToSlug(s.Title);
        }
        
        var projectsWithEmptySlug = db.Projects.Where(p => string.IsNullOrEmpty(p.Slug)).ToList();
        foreach (var p in projectsWithEmptySlug)
        {
            p.Slug = boya_usta_web.Helpers.StringHelper.ToSlug(p.Title);
        }
        
        if (servicesWithEmptySlug.Any() || projectsWithEmptySlug.Any())
        {
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken hata oluştu.");
    }
}

// Güvenlik başlıkları
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
    context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    }
    
    // Content-Security-Policy (Daha basit hali)
    context.Response.Headers.Remove("Content-Security-Policy");
    // Tek satırda CSP, modern tarayıcılar için
    context.Response.Headers.Append("Content-Security-Policy", 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval' https:; " +
        "style-src 'self' 'unsafe-inline' https:; " +
        "font-src 'self' https: data:; " +
        "img-src 'self' data: https:; " +
        "frame-src 'self' https:; " +
        "connect-src 'self' https: wss:;");

    await next();
});

// Middleware Pipeline
app.UseExceptionHandler("/hata/500");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Static files (Basit cache)
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".glb")) ctx.Context.Response.ContentType = "model/gltf-binary";
        else if (path.EndsWith(".gltf")) ctx.Context.Response.ContentType = "model/gltf+json";
        
        // 7 gün cache
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800"); 
    }
});

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePagesWithReExecute("/hata/{0}");

// Routelar
app.MapControllerRoute(name: "services-list", pattern: "hizmetler", defaults: new { controller = "Services", action = "Index" });
app.MapControllerRoute(name: "service-detail", pattern: "hizmetler/{slug}", defaults: new { controller = "Services", action = "DetailBySlug" });
app.MapControllerRoute(name: "projects-list", pattern: "projeler", defaults: new { controller = "Projects", action = "Index" });
app.MapControllerRoute(name: "project-detail", pattern: "projeler/{slug}", defaults: new { controller = "Projects", action = "DetailBySlug" });
app.MapControllerRoute(name: "contact", pattern: "iletisim", defaults: new { controller = "Contact", action = "Index" });
app.MapControllerRoute(name: "about", pattern: "hakkimizda", defaults: new { controller = "Home", action = "About" });
app.MapControllerRoute(name: "simulator", pattern: "simulator", defaults: new { controller = "Simulator", action = "Index" });
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
