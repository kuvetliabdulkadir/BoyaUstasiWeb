# Boya Ustası Web - Profesyonel Boya Badana Hizmetleri Yönetim Sistemi

## Proje Amacı

Bu web uygulaması, boya ustalarının  müşterileriyle etkili iletişim kurmasını sağlayan, profesyonel bir online iş yönetim platformudur. Uygulama, hizmet tanıtımı, proje portföyü sunumu, müşteri talep yönetimi ve online teklif alma süreçlerini dijitalleştirerek sektördeki profesyonellerin işlerini daha verimli yürütmelerine yardımcı olur.

## Hedef Kullanıcı Kitlesi

### Birincil Kullanıcılar
- **Boya Ustaları ve Firmaları:** Hizmetlerini dijital ortamda sergilemek, müşteri taleplerini yönetmek ve portföylerini sunmak isteyen profesyoneller
- **Ev Sahipleri ve Gayrimenkul Yöneticileri:** Boya hizmeti arayan, fiyat teklifi almak isteyen kişiler

### İkincil Kullanıcılar
- **İnşaat Şirketleri:** Projelerinde boya hizmeti temin etmek isteyen firmalar
- **Emlak Danışmanları:** Müşterileri için boya hizmeti koordine eden profesyoneller

## Senaryo / Kullanım Amacı

### Problem
Boya ustaları genellikle:
- Hizmetlerini etkili bir şekilde tanıtamıyor
- Müşteri taleplerini manuel takip ediyor
- Referans projelerini profesyonel şekilde sunamamaktadır

### Çözüm
Bu uygulama ile:
1. **Hizmet Tanıtımı:** Sunulan tüm boya hizmetleri detaylı açıklamalarla listelenir
2. **Proje Portföyü:** Önceki projeler "önce-sonra" fotoğraflarıyla sergilenir
3. **Online Teklif Alma:** Müşteriler detaylı form doldurarak teklif talep edebilir
4. **Renk Simülatörü:** Müşteriler renk paletlerini inceleyebilir
5. **Müşteri Yorumları:** Referanslar ve yorumlar güven oluşturur
6. **Admin Paneli:** Tüm içerik ve talepler merkezi olarak yönetilir

### Kullanım Senaryosu
1. Müşteri web sitesini ziyaret eder
2. Sunulan hizmetleri ve referans projeleri inceler
3. İletişim formunu doldurarak teklif talep eder
4. Admin panelden usta, gelen talepleri görüntüler ve yanıtlar
5. Proje tamamlandığında, yeni proje portföye eklenir

## Kullanılan Teknolojiler

| Teknoloji | Açıklama |
|-----------|----------|
| **C#** | Backend programlama dili |
| **ASP.NET Core MVC 8.0** | Web uygulama framework'ü |
| **Entity Framework Core 8.0** | ORM (Object-Relational Mapping) |
| **SQL Server Express** | Veritabanı yönetim sistemi |
| **ASP.NET Core Identity** | Kimlik doğrulama ve yetkilendirme |
| **Bootstrap 5** | Responsive UI framework |
| **JavaScript/jQuery** | Frontend interaktivite |
| **SkiaSharp** | Görsel işleme (WebP dönüşümü) |

## Proje Yapısı

```
boya_usta_web/
├── Controllers/           # MVC Controller'lar
│   ├── Admin/            # Admin panel controller'ları
│   │   ├── AccountController.cs
│   │   ├── AdminProjectsController.cs
│   │   ├── AdminServicesController.cs
│   │   ├── AdminMessagesController.cs
│   │   └── ... (15 controller)
│   ├── HomeController.cs
│   ├── ContactController.cs
│   ├── ProjectsController.cs
│   └── ServicesController.cs
├── Models/               # Veri modelleri
│   ├── Entity/          # Veritabanı entity'leri
│   │   ├── Project.cs
│   │   ├── Service.cs
│   │   ├── ContactMessage.cs
│   │   ├── Review.cs
│   │   └── ... (18 entity)
│   └── ViewModels/      # View model'ler
├── Views/               # Razor view'ları
│   ├── Home/           # Ana sayfa view'ları
│   ├── Projects/       # Proje view'ları
│   ├── Services/       # Hizmet view'ları
│   ├── Contact/        # İletişim view'ları
│   ├── Shared/         # Layout ve partial view'lar
│   └── Admin.../       # Admin panel view'ları
├── Data/
│   ├── ApplicationDbContext.cs  # EF Core DbContext
│   └── SeedData.cs              # Başlangıç verileri
├── Services/            # Business logic servisleri
├── Helpers/             # Yardımcı sınıflar
├── wwwroot/            # Statik dosyalar (CSS, JS, Images)
├── appsettings.json    # Uygulama ayarları
└── Program.cs          # Uygulama giriş noktası
```

## Veritabanı Tasarımı

### Entity-Relationship Diyagramı

```
┌─────────────────┐     ┌─────────────────┐
│    Project      │────<│  ProjectImage   │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │     │ Id (PK)         │
│ Title           │     │ ProjectId (FK)  │
│ Description     │     │ ImagePath       │
│ Category        │     │ DisplayOrder    │
│ BeforeImagePath │     └─────────────────┘
│ AfterImagePath  │
│ IsActive        │
└─────────────────┘

┌─────────────────┐     ┌─────────────────┐
│    Service      │────<│  ServiceDetail  │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │     │ Id (PK)         │
│ Title           │     │ ServiceId (FK)  │
│ ShortDescription│     │ Feature         │
│ FullDescription │     └─────────────────┘
│ IconClass       │
│ ImagePath       │
└─────────────────┘

┌─────────────────┐     ┌─────────────────┐
│ ContactMessage  │     │     Review      │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │     │ Id (PK)         │
│ FullName        │     │ CustomerName    │
│ Phone           │     │ Rating          │
│ Email           │     │ Comment         │
│ City            │     │ IsActive        │
│ ServiceType     │     └─────────────────┘
│ Message         │
│ IsRead          │
│ CreatedAt       │
└─────────────────┘

┌─────────────────┐     ┌─────────────────┐
│  ColorPalette   │     │   SiteSetting   │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │     │ Id (PK)         │
│ Name            │     │ Key (Unique)    │
│ HexCode         │     │ Value           │
│ Category        │     │ GroupName       │
└─────────────────┘     └─────────────────┘
```



## Ekran Görüntüleri

### Kullanıcı Arayüzü
- **Ana Sayfa:** Hero slider, hizmetler, öne çıkan projeler, istatistikler
- **Hizmetler Sayfası:** Tüm hizmetlerin detaylı listesi
- **Projeler Sayfası:** Kategori filtreleme ile proje galerisi
- **İletişim:** Detaylı teklif talep formu

### Admin Paneli
- **Dashboard:** Genel istatistikler ve son aktiviteler
- **Projeler Yönetimi:** Proje ekleme, düzenleme, silme
- **Hizmetler Yönetimi:** Hizmet CRUD işlemleri
- **Mesaj Yönetimi:** Gelen talepleri görüntüleme ve yanıtlama
- **Ayarlar:** Site ayarları, SEO, renk paleti yönetimi


## Tanıtım Videosu

**YouTube Linki:** [Proje Tanıtım Videosu](#)


## Proje Özellikleri

### Tamamlanan Özellikler
-  MVC mimarisi ile katmanlı yapı
-  Entity Framework Core ile veritabanı entegrasyonu
-  ASP.NET Core Identity ile kimlik doğrulama
-  Responsive tasarım (Bootstrap 5)
-  SEO uyumlu yapı (meta tags, sitemap)
-  Admin paneli ile tam CRUD işlemleri
-  Görsel optimizasyon (WebP dönüşümü)
-  İletişim formu ve mesaj yönetimi
-  Proje portföyü (önce/sonra görselleri)
-  Müşteri yorumları sistemi
-  Renk paleti simülatörü
-  FAQ (Sıkça Sorulan Sorular) bölümü





> Bu proje eğitim ve ticari amaçlı geliştirilmiştir.

