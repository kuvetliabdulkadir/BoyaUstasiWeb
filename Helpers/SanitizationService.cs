using Ganss.Xss;

namespace boya_usta_web.Helpers
{
    public static class SanitizationService
    {
        private static readonly HtmlSanitizer Sanitizer = new HtmlSanitizer();

        public static string? Sanitize(string? input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return Sanitizer.Sanitize(input);
        }
    }
}
