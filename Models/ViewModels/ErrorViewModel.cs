namespace boya_usta_web.Models.ViewModels;

// Hata sayfası için ViewModel
public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
