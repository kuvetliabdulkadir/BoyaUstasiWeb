using System.Text.Json;
using System.Text.Json.Serialization;
using boya_usta_web.Models;
using Microsoft.Extensions.Logging;

namespace boya_usta_web.Services
{
    // Google Places API kullanarak yorumları yöneten servis
    public class GoogleReviewsService : IGoogleReviewsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GoogleReviewsService> _logger;

        public GoogleReviewsService(HttpClient httpClient, ILogger<GoogleReviewsService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // Google Places API'den yorumları çeker (Place ID ve API Key ile)
        public async Task<List<Review>> FetchReviewsFromGoogleAsync(string placeId, string apiKey)
        {
            var reviews = new List<Review>();

            try
            {
                // Google Places API Details endpoint URL'ini oluştur
                var url = $"https://maps.googleapis.com/maps/api/place/details/json?place_id={placeId}&fields=reviews&key={apiKey}";

                // API'ye istek at
                var response = await _httpClient.GetStringAsync(url);
                var jsonDoc = JsonDocument.Parse(response);

                // JSON yanıtını işle
                if (jsonDoc.RootElement.TryGetProperty("result", out var result))
                {
                    if (result.TryGetProperty("reviews", out var reviewsArray))
                    {
                        foreach (var reviewElement in reviewsArray.EnumerateArray())
                        {
                            try
                            {
                                // Her bir yorumu Review nesnesine dönüştür
                                var review = new Review
                                {
                                    CustomerName = reviewElement.TryGetProperty("author_name", out var authorName) 
                                        ? authorName.GetString() ?? "Anonim" 
                                        : "Anonim",
                                    Comment = reviewElement.TryGetProperty("text", out var text) 
                                        ? text.GetString() ?? "" 
                                        : "",
                                    Rating = reviewElement.TryGetProperty("rating", out var rating) 
                                        ? rating.GetInt32() 
                                        : 5,
                                    ReviewDate = reviewElement.TryGetProperty("time", out var time) 
                                        ? DateTimeOffset.FromUnixTimeSeconds(time.GetInt64()).DateTime 
                                        : DateTime.UtcNow,
                                    Source = "Google",
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow
                                };

                                // Yorum metni çok uzunsa kısalt (Veritabanı sınırı veya UI için)
                                if (review.Comment.Length > 500)
                                {
                                    review.Comment = review.Comment.Substring(0, 497) + "...";
                                }

                                reviews.Add(review);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Tekil yorum parse edilemedi, atlanıyor.");
                            }
                        }
                    }
                }

                // API hata mesajı döndürdüyse logla
                if (jsonDoc.RootElement.TryGetProperty("error_message", out var errorMessage))
                {
                    _logger.LogError("Google API Hatası: {Error}", errorMessage.GetString());
                    throw new Exception($"Google API Hatası: {errorMessage.GetString()}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Google yorumları çekilirken genel hata oluştu");
                throw;
            }

            return reviews;
        }

        // JSON verisinden yorumları içe aktarır (Manuel import veya farklı kaynaklar için)
        public Task<List<Review>> ImportReviewsFromJsonAsync(string jsonData)
        {
            var reviews = new List<Review>();

            try
            {
                var jsonDoc = JsonDocument.Parse(jsonData);
                JsonElement reviewsArray;
                
                // Farklı JSON formatlarını destekle (Dizi veya Nesne içinde dizi)
                if (jsonDoc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    reviewsArray = jsonDoc.RootElement;
                }
                else if (jsonDoc.RootElement.TryGetProperty("reviews", out reviewsArray))
                {
                    // Standart format
                }
                else if (jsonDoc.RootElement.TryGetProperty("result", out var result) && 
                         result.TryGetProperty("reviews", out reviewsArray))
                {
                    // Google Places API ham yanıt formatı
                }
                else
                {
                    throw new Exception("Geçersiz JSON formatı: 'reviews' dizisi bulunamadı.");
                }

                // Yorumları döngüyle işle
                foreach (var reviewElement in reviewsArray.EnumerateArray())
                {
                    try
                    {
                        var review = new Review
                        {
                            // Farklı isimlendirme standartlarını (camelCase, PascalCase) destekle
                            CustomerName = GetJsonString(reviewElement, "author_name") ?? 
                                         GetJsonString(reviewElement, "customerName") ?? "Anonim",
                            
                            Comment = GetJsonString(reviewElement, "text") ?? 
                                     GetJsonString(reviewElement, "comment") ?? "",
                            
                            Rating = GetJsonInt(reviewElement, "rating") ?? 
                                    GetJsonInt(reviewElement, "Rating") ?? 5,
                            
                            Location = GetJsonString(reviewElement, "location"),
                            
                            Source = GetJsonString(reviewElement, "source") ?? "Google",
                            
                            ReviewDate = GetJsonDateTime(reviewElement, "time") ?? 
                                        GetJsonDateTimeFromString(reviewElement, "reviewDate") ?? DateTime.UtcNow,
                            
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        if (review.Comment.Length > 500)
                        {
                            review.Comment = review.Comment.Substring(0, 497) + "...";
                        }

                        reviews.Add(review);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "JSON import sırasında bir yorum parse edilemedi.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JSON import işlemi başarısız oldu.");
                throw;
            }

            return Task.FromResult(reviews);
        }

        // Helper metodlar (Kod tekrarını önlemek için)
        private string? GetJsonString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        private int? GetJsonInt(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetInt32() : null;
        }

        private DateTime? GetJsonDateTime(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) 
                ? DateTimeOffset.FromUnixTimeSeconds(prop.GetInt64()).DateTime 
                : null;
        }

        private DateTime? GetJsonDateTimeFromString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && DateTime.TryParse(prop.GetString(), out var date)
                ? date
                : null;
        }
    }
}

