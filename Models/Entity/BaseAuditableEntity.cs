namespace boya_usta_web.Models
{
    // Oluşturulma ve güncellenme tarihleri olan entity'ler için temel sınıf
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

