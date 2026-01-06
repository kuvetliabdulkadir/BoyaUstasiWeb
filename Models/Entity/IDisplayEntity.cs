namespace boya_usta_web.Models
{
    public interface IDisplayEntity
    {
        bool IsActive { get; set; }
        int DisplayOrder { get; set; }
    }
}
