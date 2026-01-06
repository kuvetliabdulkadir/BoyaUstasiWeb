namespace boya_usta_web.Models
{
    // CreatedAt olmayan ama IsActive, DisplayOrder ve UpdatedAt olan entity'ler için temel sınıf
    public abstract class BaseDisplayEntityWithoutCreatedAt : BaseEntity, IDisplayEntity
    {
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

