using System.ComponentModel.DataAnnotations;

namespace boya_usta_web.Models
{
    // Proje galerisi için ek görsel varlığı.
    public class ProjectImage
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Project? Project { get; set; }
    }
}

