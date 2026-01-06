using boya_usta_web.Models;

namespace boya_usta_web.Services
{
    // Google yorumlarını çekme ve işleme servis arayüzü
    public interface IGoogleReviewsService
    {
        // Google'dan yorumları çeker
        Task<List<Review>> FetchReviewsFromGoogleAsync(string placeId, string apiKey);
        
        // JSON verisinden yorumları içe aktarır
        Task<List<Review>> ImportReviewsFromJsonAsync(string jsonData);
    }
}

