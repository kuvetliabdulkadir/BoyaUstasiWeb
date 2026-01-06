namespace boya_usta_web.Models
{
    // Aktiflik ve sıralama özellikleri olan entity'ler için temel sınıf
    public abstract class BaseDisplayEntity : BaseAuditableEntity, IDisplayEntity
    {
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public string? Slug { get; set; }
    }
}

