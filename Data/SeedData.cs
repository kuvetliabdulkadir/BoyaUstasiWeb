using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Models;

namespace boya_usta_web.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var env = serviceProvider.GetService<IWebHostEnvironment>();

            // Migration'ları uygula (EnsureCreated + Migration çakışmasını önler)
            await context.Database.MigrateAsync();

            // Admin rolü oluştur
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Admin kullanıcısı oluştur
            var adminUsername = configuration["AdminSettings:AdminUsername"] ?? "admin";
            var adminEmail = configuration["AdminSettings:AdminEmail"] ?? "admin@example.com";
            var seedAdminPassword = configuration["AdminSettings:SeedAdminPassword"] ?? "Admin123!";
            var resetPasswordOnStartup = configuration.GetValue<bool>("AdminSettings:ResetAdminPasswordOnStartup");
            
            // ÖNEMLİ: Mevcut Admin rolündeki diğer kullanıcıları temizle (tek admin kuralı)
            var adminsInRole = await userManager.GetUsersInRoleAsync("Admin");
            foreach (var user in adminsInRole)
            {
                if (user.UserName != adminUsername && user.Email != adminEmail)
                {
                    await userManager.DeleteAsync(user);
                }
            }
            
            var adminUser = await userManager.FindByNameAsync(adminUsername);
            if (adminUser == null)
            {
                // Email ile de kontrol et (eski kullanıcılar için)
                adminUser = await userManager.FindByEmailAsync(adminEmail);
            }

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminUsername,
                    Email = adminEmail,
                    FullName = "Site Yöneticisi",
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(adminUser, seedAdminPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(" | ", createResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Admin kullanıcısı oluşturulamadı: {errors}");
                }
            }
            else
            {
                // Kullanıcı zaten varsa, bilgilerini GÜNCELLE (Şifre ve Email dahil)
                bool needUpdate = false;

                if (adminUser.UserName != adminUsername)
                {
                    adminUser.UserName = adminUsername;
                    needUpdate = true;
                }

                if (adminUser.Email != adminEmail)
                {
                    adminUser.Email = adminEmail;
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    await userManager.UpdateAsync(adminUser);
                }

                // Şifreyi her zaman zorla güncelle (kullanıcının istediği şifre olsun)
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await userManager.ResetPasswordAsync(adminUser, token, seedAdminPassword);
                if (!resetResult.Succeeded)
                {
                    // Eğer şifre gereksinimlere uymuyorsa hata logla
                    var errors = string.Join(" | ", resetResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    // Log.Warning veya console'a yazılabilir. Burada sessizce başarısız olmasını engellemek için:
                    if (!resetResult.Errors.Any(e => e.Code == "PasswordMismatch")) // PasswordMismatch bazen yanıltıcı olabilir
                    {
                        // Hata varsa bile devam et ama logla
                        Console.WriteLine($"UYARI: Admin şifresi güncellenemedi: {errors}");
                    }
                }
            }

            // Admin rolünü garanti et
            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Development veya Production fark etmeksizin, ayar true ise şifreyi resetle
            if (resetPasswordOnStartup)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                var resetResult = await userManager.ResetPasswordAsync(adminUser, token, seedAdminPassword);
                if (!resetResult.Succeeded)
                {
                    var errors = string.Join(" | ", resetResult.Errors.Select(e => $"{e.Code}:{e.Description}"));
                    throw new InvalidOperationException($"Admin şifresi resetlenemedi: {errors}");
                }
            }

           
            // Site ayarları
            if (!context.SiteSettings.Any())
            {
                var settings = new List<SiteSetting>
                {
                    // Genel
                    new SiteSetting { Key = "SiteName", Value = "Bursa Boya Ustası", Group = "Genel" },
                    new SiteSetting { Key = "SiteSlogan", Value = "Profesyonel İç ve Dış Cephe Boyama", Group = "Genel" },
                    new SiteSetting { Key = "LogoPath", Value = "/images/logo.png", Group = "Genel" },
                    new SiteSetting { Key = "FaviconPath", Value = "/favicon.ico", Group = "Genel" },
                    new SiteSetting { Key = "ExperienceYears", Value = "15", Group = "Genel" },
                    new SiteSetting { Key = "MetaDescription", Value = "Bursa'da profesyonel boya hizmetleri. İç cephe, dış cephe, dekoratif boyama. {years} yıl tecrübe, 2 yıl garanti. Ücretsiz keşif.", Group = "Genel" },
                    new SiteSetting { Key = "FooterAboutShort", Value = "{years} yıllık tecrübemizle Bursa'da profesyonel boya hizmetleri sunuyoruz. Kaliteli malzeme, uygun fiyat ve 2 yıl garanti.", Group = "Genel" },
                    new SiteSetting { Key = "HeroExperienceBadge", Value = "{years}+ Yıl", Group = "Genel" },


                    // Ana Sayfa - Neden Biz
                    new SiteSetting { Key = "WhyUsSubtitle", Value = "Neden Bizi Seçmelisiniz?", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsTitle", Value = "Kalite ve Güvenin Adresi", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsExperienceDescription", Value = "{years} yıllık tecrübemizle size en iyi hizmeti sunuyoruz", Group = "Genel" },
                    new SiteSetting { Key = "WhyUsCard1Title", Value = "Kaliteli Malzeme", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard1Desc", Value = "Marshall, Filli Boya, Dyo gibi premium markaları kullanıyoruz.", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard1Icon", Value = "bi bi-bucket-fill", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard2Title", Value = "Deneyimli Ekip", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "TeamExperienceDescription", Value = "{years} yılı aşkın tecrübeye sahip uzman ekibimizle hizmetinizdeyiz.", Group = "Genel" },
                    new SiteSetting { Key = "WhyUsCard2Icon", Value = "bi bi-people-fill", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard3Title", Value = "Uygun Fiyat", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard3Desc", Value = "Kaliteden ödün vermeden en uygun fiyatları sunuyoruz.", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard3Icon", Value = "bi bi-cash-coin", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard4Title", Value = "2 Yıl Garanti", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard4Desc", Value = "Tüm işlerimize 2 yıl garanti veriyoruz. Sorunsuz hizmet.", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "WhyUsCard4Icon", Value = "bi bi-shield-check", Group = "Ana Sayfa" },

                    // Ana Sayfa - Hero ikonları
                    new SiteSetting { Key = "HeroFeature1Icon", Value = "bi bi-shield-check", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "HeroFeature2Icon", Value = "bi bi-cash-coin", Group = "Ana Sayfa" },
                    new SiteSetting { Key = "HeroFeature3Icon", Value = "bi bi-award", Group = "Ana Sayfa" },

                    // Hakkımızda
                    new SiteSetting { Key = "AboutTitle", Value = "{years} Yıllık Tecrübe ile Yanınızdayız", Group = "Hakkımızda" },
                    new SiteSetting { Key = "AboutText", Value = "Bursa'da profesyonel boya hizmetleri sunuyoruz. Müşteri memnuniyetini ön planda tutarak, kaliteli malzeme ve işçilikle sizlere hizmet veriyoruz.", Group = "Hakkımızda" },
                    new SiteSetting { Key = "ExperienceBadgeYears", Value = "15+", Group = "Hakkımızda" },
                    new SiteSetting { Key = "ExperienceBadgeLabel", Value = "Yıl Tecrübe", Group = "Hakkımızda" },

                    // İletişim
                    new SiteSetting { Key = "Phone", Value = "+90 532 XXX XX XX", Group = "İletişim" },
                    new SiteSetting { Key = "WhatsApp", Value = "+90 532 XXX XX XX", Group = "İletişim" },
                    new SiteSetting { Key = "Email", Value = "info@boyaustasi.com", Group = "İletişim" },
                    new SiteSetting { Key = "Address", Value = "Görükle, Bursa", Group = "İletişim" },
                    new SiteSetting { Key = "WorkingHours", Value = "Pazartesi - Cumartesi: 08:00 - 19:00", Group = "İletişim" },
                    new SiteSetting { Key = "GoogleMapsEmbed", Value = "", Group = "İletişim" },

                    // Sosyal Medya
                    new SiteSetting { Key = "Facebook", Value = "", Group = "Sosyal Medya" },
                    new SiteSetting { Key = "Instagram", Value = "", Group = "Sosyal Medya" },
                    new SiteSetting { Key = "YouTube", Value = "", Group = "Sosyal Medya" },
                    new SiteSetting { Key = "LinkedIn", Value = "", Group = "Sosyal Medya" },
                    new SiteSetting { Key = "TikTok", Value = "", Group = "Sosyal Medya" },

                    // KVKK/Gizlilik
                    new SiteSetting { Key = "PrivacyPolicy", Value = "", Group = "Yasal" },
                    new SiteSetting { Key = "CookiePolicy", Value = "", Group = "Yasal" }
                };
                context.SiteSettings.AddRange(settings);
            }

            // Var olan kurulumlarda eksik ayar anahtarlarını tamamla (kırmadan)
            await EnsureSettingAsync(context, "ExperienceYears", "15", "Genel");
            await EnsureSettingAsync(context, "WhatsAppDefaultMessage", "Merhaba, boya hizmeti hakkında bilgi almak istiyorum.", "İletişim");
            await EnsureSettingAsync(context, "MetaDescription", "Bursa'da profesyonel boya hizmetleri. İç cephe, dış cephe, dekoratif boyama. {years} yıl tecrübe, 2 yıl garanti. Ücretsiz keşif.", "Genel");
            await EnsureSettingAsync(context, "FooterAboutShort", "{years} yıllık tecrübemizle Bursa'da profesyonel boya hizmetleri sunuyoruz. Kaliteli malzeme, uygun fiyat ve 2 yıl garanti.", "Genel");
            await EnsureSettingAsync(context, "HeroExperienceBadge", "{years}+ Yıl", "Genel");

            await EnsureSettingAsync(context, "WhyUsSubtitle", "Neden Bizi Seçmelisiniz?", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsTitle", "Kalite ve Güvenin Adresi", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsExperienceDescription", "{years} yıllık tecrübemizle size en iyi hizmeti sunuyoruz", "Genel");
            await EnsureSettingAsync(context, "WhyUsCard1Title", "Kaliteli Malzeme", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard1Desc", "Marshall, Filli Boya, Dyo gibi premium markaları kullanıyoruz.", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard1Icon", "bi bi-bucket-fill", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard2Title", "Deneyimli Ekip", "Ana Sayfa");
            await EnsureSettingAsync(context, "TeamExperienceDescription", "{years} yılı aşkın tecrübeye sahip uzman ekibimizle hizmetinizdeyiz.", "Genel");
            await EnsureSettingAsync(context, "WhyUsCard2Icon", "bi bi-people-fill", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard3Title", "Uygun Fiyat", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard3Desc", "Kaliteden ödün vermeden en uygun fiyatları sunuyoruz.", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard3Icon", "bi bi-cash-coin", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard4Title", "2 Yıl Garanti", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard4Desc", "Tüm işlerimize 2 yıl garanti veriyoruz. Sorunsuz hizmet.", "Ana Sayfa");
            await EnsureSettingAsync(context, "WhyUsCard4Icon", "bi bi-shield-check", "Ana Sayfa");

            await EnsureSettingAsync(context, "HeroFeature1Icon", "bi bi-shield-check", "Ana Sayfa");
            await EnsureSettingAsync(context, "HeroFeature2Icon", "bi bi-cash-coin", "Ana Sayfa");
            await EnsureSettingAsync(context, "HeroFeature3Icon", "bi bi-award", "Ana Sayfa");
            await EnsureSettingAsync(context, "ExperienceBadgeYears", "15+", "Hakkımızda");
            await EnsureSettingAsync(context, "ExperienceBadgeLabel", "Yıl Tecrübe", "Hakkımızda");
            await EnsureSettingAsync(context, "SeoGoogleAnalyticsId", "G-YHCDQJ78Z1", "SEO");
            await EnsureSettingAsync(context, "SeoSiteUrl", "https://inegolboyaustasi.com", "SEO");
            await EnsureSettingAsync(context, "SeoSiteName", "Bursa Boya Ustası", "SEO");
            await EnsureSettingAsync(context, "SeoDefaultTitle", "Bursa Boya Ustası - Profesyonel Boya Badana Hizmetleri", "SEO");
            await EnsureSettingAsync(context, "SeoDefaultDescription", "Bursa'da profesyonel boya badana hizmetleri. İç cephe, dış cephe, dekoratif boya. Ücretsiz keşif, 2 yıl garanti.", "SEO");
            await EnsureSettingAsync(context, "SeoDefaultKeywords", "bursa boya ustası, bursa boyacı, boya badana, ev boyama, iç cephe boya, dış cephe boya", "SEO");
            await EnsureSettingAsync(context, "SeoDefaultOgImage", "/images/og-image.png", "SEO");
            await EnsureSettingAsync(context, "SeoThemeColor", "#2C5F8C", "SEO");
            await EnsureSettingAsync(context, "SeoLocale", "tr_TR", "SEO");
            await EnsureSettingAsync(context, "SeoGeoRegion", "TR-16", "SEO");
            await EnsureSettingAsync(context, "SeoGeoPlacename", "Bursa", "SEO");
            await EnsureSettingAsync(context, "SeoGeoLatitude", "40.1885", "SEO");
            await EnsureSettingAsync(context, "SeoGeoLongitude", "29.0610", "SEO");
            await EnsureSettingAsync(context, "SeoRobotsDefault", "index, follow", "SEO");

            // Popup ayarı
            
            if (!context.PopupSettings.Any())
            {
                var popup = new PopupSetting
                {
                    Title = "İlk Müşterilerimize %10 İndirim!",
                    Content = "Bizi ilk kez tercih eden müşterilerimize özel %10 indirim fırsatını kaçırmayın.",
                    ButtonText = "Teklif Al",
                    ButtonUrl = "/iletisim",
                    IsActive = true,
                    DelaySeconds = 5,
                    DontShowAgainDays = 30
                };
                context.PopupSettings.Add(popup);
            }
            

            await context.SaveChangesAsync();

            // Sayfa SEO ayarları
            if (!context.PageSeoSettings.Any())
            {
                var pageSeoSettings = new List<PageSeoSetting>
                {
                    new PageSeoSetting
                    {
                        PagePath = "/",
                        PageName = "Ana Sayfa",
                        Title = "Bursa Boya Ustası - Profesyonel Boya Badana Hizmetleri",
                        MetaDescription = "Bursa'da profesyonel boya badana hizmetleri. İç cephe, dış cephe, dekoratif boya. Ücretsiz keşif, 2 yıl garanti.",
                        Keywords = "bursa boya ustası, bursa boyacı, boya badana, ev boyama, iç cephe boya, dış cephe boya",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 1
                    },
                    new PageSeoSetting
                    {
                        PagePath = "/hizmetler",
                        PageName = "Hizmetler",
                        Title = "Boya Hizmetlerimiz - Bursa Boya Ustası",
                        MetaDescription = "İç cephe, dış cephe, dekoratif boya hizmetlerimizi inceleyin. Bursa'da profesyonel boya badana.",
                        Keywords = "boya hizmetleri, iç cephe boya, dış cephe boya, dekoratif boya, bursa boyacı",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 2
                    },
                    new PageSeoSetting
                    {
                        PagePath = "/projeler",
                        PageName = "Projeler",
                        Title = "Tamamlanan Projeler - Bursa Boya Ustası",
                        MetaDescription = "Tamamladığımız boya projelerini inceleyin. Ev, apartman, villa ve işyeri boyama referanslarımız.",
                        Keywords = "boya projeleri, referanslar, ev boyama, apartman boyama, bursa boya",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 3
                    },
                    new PageSeoSetting
                    {
                        PagePath = "/simulator",
                        PageName = "Renk Simülatörü",
                        Title = "Renk Simülatörü - Duvarınızı Boyamadan Önce Görün",
                        MetaDescription = "Yapay zeka destekli renk simülatörü ile duvarlarınızın yeni rengini boyamadan önce görün. Fotoğraf yükleyin, renk deneyin.",
                        Keywords = "renk simülatörü, duvar rengi, boya rengi seçimi, yapay zeka boya",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 4
                    },
                    new PageSeoSetting
                    {
                        PagePath = "/iletisim",
                        PageName = "İletişim",
                        Title = "İletişim - Bursa Boya Ustası",
                        MetaDescription = "Bursa Boya Ustası ile iletişime geçin. Ücretsiz keşif ve fiyat teklifi alın. WhatsApp, telefon ve form ile ulaşın.",
                        Keywords = "bursa boya ustası iletişim, boya teklifi, ücretsiz keşif, bursa boyacı telefon",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 5
                    },
                    new PageSeoSetting
                    {
                        PagePath = "/hakkimizda",
                        PageName = "Hakkımızda",
                        Title = "Hakkımızda - Bursa Boya Ustası",
                        MetaDescription = "Bursa Boya Ustası hakkında bilgi edinin. Yılların tecrübesi, profesyonel ekip ve kaliteli hizmet anlayışımız.",
                        Keywords = "bursa boya ustası hakkında, boya ustası ekibi, profesyonel boyacı bursa",
                        OgType = "website",
                        IsSystemPage = true,
                        DisplayOrder = 6
                    }
                };

                context.PageSeoSettings.AddRange(pageSeoSettings);
                await context.SaveChangesAsync();
            }
        }

        private static async Task EnsureSettingAsync(ApplicationDbContext context, string key, string defaultValue, string group)
        {
            var exists = await context.SiteSettings.AnyAsync(s => s.Key == key);
            if (exists) return;

            context.SiteSettings.Add(new SiteSetting
            {
                Key = key,
                Value = defaultValue,
                Group = group,
                UpdatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
        }
    }
}

