using System.Text.RegularExpressions;

namespace boya_usta_web.Helpers
{
    // Settings Dictionary için extension method'lar
    // GetValueOrDefault kullanımını kısaltır ve daha okunabilir hale getirir
    public static class SettingsExtensions
    {
        // Settings dictionary'den değer alır, yoksa default değer döndürür
        public static string GetSetting(this Dictionary<string, string?> settings, string key, string defaultValue = "")
        {
            return settings.GetValueOrDefault(key, defaultValue) ?? defaultValue;
        }

        // Settings dictionary'den değer alır ve içindeki placeholder'ları değiştirir
        // Örnek: "{years}" → ExperienceYears değeri ile değiştirilir
        public static string GetSettingWithReplace(this Dictionary<string, string?> settings, string key, string replaceKey, string defaultValue = "")
        {
            var value = settings.GetSetting(key, defaultValue);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            var replaceValue = settings.GetSetting(replaceKey, "");
            return value.Replace("{" + replaceKey + "}", replaceValue);
        }

        // Settings dictionary'den değer alır ve birden fazla placeholder'ı değiştirir
        public static string GetSettingWithReplacements(this Dictionary<string, string?> settings, string key, Dictionary<string, string> replacements, string defaultValue = "")
        {
            var value = settings.GetSetting(key, defaultValue);
            if (string.IsNullOrEmpty(value))
                return defaultValue;

            foreach (var replacement in replacements)
            {
                value = value.Replace("{" + replacement.Key + "}", replacement.Value);
            }

            return value;
        }
    }
}

