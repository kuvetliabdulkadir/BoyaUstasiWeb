using System.Text.Json;

namespace boya_usta_web.Helpers
{
    // İl-İlçe mapping işlemleri için helper sınıfı
    // CityDistrictsMap JSON formatını parse eder ve Dictionary döndürür
    public static class CityDistrictsHelper
    {
        // Settings'den Cities ayarını alır ve liste olarak döndürür
        public static List<string> GetCities(Dictionary<string, string?> settings)
        {
            var citiesValue = settings.GetValueOrDefault("Cities", "");
            if (string.IsNullOrWhiteSpace(citiesValue))
            {
                // Varsayılan olarak Bursa'yı döndür
                return new List<string> { "Bursa" };
            }

            return citiesValue
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .OrderBy(c => c)
                .ToList();
        }

        // İl listesinden varsayılan şehri döndürür (ilk şehir veya "Bursa")
        public static string GetDefaultCity(List<string> cities)
        {
            if (cities == null || !cities.Any())
            {
                return "Bursa";
            }

            // Önce "Bursa" varsa onu döndür, yoksa ilk şehri döndür
            var bursa = cities.FirstOrDefault(c => c.Equals("Bursa", StringComparison.OrdinalIgnoreCase));
            return bursa ?? cities.First();
        }

        // Settings'den CityDistrictsMap'i alır ve Dictionary'ye dönüştürür
        // Önce JSON formatını kontrol eder, yoksa eski ServiceAreas sistemini kullanır
        public static Dictionary<string, List<string>> GetCityDistrictsMap(
            Dictionary<string, string?> settings,
            List<string> cities)
        {
            var cityDistrictsMap = new Dictionary<string, List<string>>();

            // Önce JSON formatındaki CityDistrictsMap'i kontrol et
            var cityDistrictsMapJson = settings.GetValueOrDefault("CityDistrictsMap", "");
            if (!string.IsNullOrWhiteSpace(cityDistrictsMapJson))
            {
                try
                {
                    var jsonMap = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(cityDistrictsMapJson);
                    if (jsonMap != null && jsonMap.Any())
                    {
                        cityDistrictsMap = jsonMap;
                    }
                }
                catch
                {
                    // JSON parse hatası durumunda boş map döndür
                    cityDistrictsMap = new Dictionary<string, List<string>>();
                }
            }

            // Eğer JSON yoksa veya boşsa eski sistemi kullan (geriye dönük uyumluluk)
            if (!cityDistrictsMap.Any())
            {
                // Hizmet Bölgeleri ayarından ilçe listesini çek (Bursa için)
                var serviceAreas = settings.GetValueOrDefault("ServiceAreas", "");
                var bursaDistricts = new List<string>();

                if (!string.IsNullOrWhiteSpace(serviceAreas))
                {
                    // Virgülle ayrılmış bölgeleri parse et
                    bursaDistricts = serviceAreas
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrWhiteSpace(d))
                        .OrderBy(d => d)
                        .ToList();
                }

                // Eğer ayar yoksa veya boşsa varsayılan Bursa ilçelerini kullan
                if (!bursaDistricts.Any())
                {
                    bursaDistricts = new List<string>
                    {
                        "Nilüfer", "Osmangazi", "Yıldırım", "Görükle", "Mudanya", "Gemlik", "Gürsu", "Kestel", "Diğer"
                    };
                }

                // Her il için ilçe listesini oluştur
                foreach (var city in cities)
                {
                    if (city.Equals("Bursa", StringComparison.OrdinalIgnoreCase))
                    {
                        cityDistrictsMap[city] = bursaDistricts;
                    }
                    else
                    {
                        // Diğer iller için varsayılan ilçeler (şimdilik boş veya "Diğer")
                        cityDistrictsMap[city] = new List<string> { "Diğer" };
                    }
                }
            }
            else
            {
                // JSON'dan gelen map'te olmayan iller için "Diğer" ekle
                foreach (var city in cities)
                {
                    if (!cityDistrictsMap.ContainsKey(city))
                    {
                        cityDistrictsMap[city] = new List<string> { "Diğer" };
                    }
                }
            }

            return cityDistrictsMap;
        }

        // Tüm ilçeleri tek bir liste olarak döndürür (About sayfası için)
        // Format: "İl - İlçe"
        public static List<string> GetAllDistrictsAsList(Dictionary<string, List<string>> cityDistrictsMap)
        {
            var allDistricts = new List<string>();

            foreach (var city in cityDistrictsMap.Keys.OrderBy(c => c))
            {
                var districts = cityDistrictsMap[city];
                foreach (var district in districts.OrderBy(d => d))
                {
                    allDistricts.Add($"{city} - {district}");
                }
            }

            return allDistricts;
        }
    }
}

