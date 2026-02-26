using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Data;
using boya_usta_web.Models;
using boya_usta_web.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// --- Sentry Entegrasyonu ---
builder.WebHost.UseSentry(o =>
{
    o.Dsn = builder.Configuration["Sentry:Dsn"] ?? "";
    o.Debug = builder.Environment.IsDevelopment();
    o.TracesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.2;
    o.Environment = builder.Environment.EnvironmentName;
    
    // Versiyon bilgisini appsettings'den çekmek daha güvenlidir
    o.Release = builder.Configuration["App:Version"] ?? "1.0.0";
    
    o.ProfilesSampleRate = builder.Environment.IsDevelopment() ? 1.0 : 0.1;
    o.SendDefaultPii = false; 
    o.MaxBreadcrumbs = 50;
});

// --- Database & Auth ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 3;
    options.Lockout.AllowedForNewUsers = true;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --- Güvenlik: Data Protection ---
// GitHub'da görünmemesi gereken özel klasör yollarını değişkene aldık
var keysDirectory = builder.Configuration["Security:KeysPath"] ?? "Data/keys";
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, keysDirectory)))
    .SetApplicationName("BoyaUstaWeb");

// --- Cookie Ayarları ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() 
        ? CookieSecurePolicy.SameAsRequest 
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    
    // Admin panel yolunu appsettings'den yönetmek brute-force riskini azaltır
    var adminPath = builder.Configuration["AdminSettings:Path"] ?? "/admin";
    options.LoginPath = $"{adminPath}/giris";
    options.LogoutPath = $"{adminPath}/cikis";
    options.AccessDeniedPath = "/Error/403";
});

builder.Services.AddAntiforgery(options => options.HeaderName = "X-CSRF-TOKEN");

// --- Dependency Injection ---
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

builder.Services.AddControllersWithViews(options =>
{
    options.MaxModelBindingCollectionSize = int.MaxValue;
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 60 * 1024 * 1024; // 60 MB
});

var app = builder.Build();

// --- Database Initialization ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
        // Slug onarımı vb. işlemleri burada yapmaya devam edebilirsiniz.
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu.");
    }
}

// --- Güvenlik Başlıkları (Middleware) ---
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    if (!app.Environment.IsDevelopment())
    {
        context.Response.Headers.Append("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
    }
    
    // Content-Security-Policy
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

app.UseExceptionHandler("/hata/500");
if (!app.Environment.IsDevelopment()) app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.File.Name.ToLowerInvariant();
        if (path.EndsWith(".glb")) ctx.Context.Response.ContentType = "model/gltf-binary";
        // Cache süresi üretim ortamında artırılabilir
        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=604800"); 
    }
});

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePagesWithReExecute("/hata/{0}");

// --- Route Tanımlamaları ---
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
