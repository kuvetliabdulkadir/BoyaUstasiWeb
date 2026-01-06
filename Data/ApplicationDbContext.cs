using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using boya_usta_web.Models;

namespace boya_usta_web.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceDetail> ServiceDetails { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<Statistic> Statistics { get; set; }
        public DbSet<ColorPalette> ColorPalettes { get; set; }
        public DbSet<PopupSetting> PopupSettings { get; set; }
        public DbSet<PageSeoSetting> PageSeoSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Project - ProjectImage ilişkisi
            builder.Entity<ProjectImage>()
                .HasOne(pi => pi.Project)
                .WithMany(p => p.AdditionalImages)
                .HasForeignKey(pi => pi.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Service - ServiceDetail ilişkisi
            builder.Entity<ServiceDetail>()
                .HasOne(sd => sd.Service)
                .WithMany(s => s.Details)
                .HasForeignKey(sd => sd.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // SiteSetting için unique key
            builder.Entity<SiteSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            // İndeksler
            builder.Entity<Project>()
                .HasIndex(p => p.Category);

            builder.Entity<Project>()
                .HasIndex(p => p.IsActive);

            builder.Entity<Review>()
                .HasIndex(r => r.IsActive);

            builder.Entity<ContactMessage>()
                .HasIndex(c => c.IsRead);

            // ContactMessage alanlarını nullable olarak yapılandır
            builder.Entity<ContactMessage>(entity =>
            {
                entity.Property(e => e.FullName).IsRequired(false);
                entity.Property(e => e.Phone).IsRequired(false);
                entity.Property(e => e.City).IsRequired(false);
                entity.Property(e => e.District).IsRequired(false);
                entity.Property(e => e.ServiceType).IsRequired(false);
                entity.Property(e => e.Message).IsRequired(false);
            });

            builder.Entity<ColorPalette>()
                .HasIndex(c => c.Category);

            // PageSeoSetting için unique index (PagePath)
            builder.Entity<PageSeoSetting>()
                .HasIndex(p => p.PagePath)
                .IsUnique();
        }
    }
}

