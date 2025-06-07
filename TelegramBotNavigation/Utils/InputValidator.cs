using System.Text.RegularExpressions;

namespace TelegramBotNavigation.Utils
{
    public static class InputValidator
    {
        private static readonly Regex UrlRegex = new Regex(@"^(https?:\/\/)[^\s$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidUrl(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return Uri.TryCreate(input, UriKind.Absolute, out var uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsValidUrlRegex(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            return UrlRegex.IsMatch(input);
        }

        public static bool IsValidText(string input, int maxLength = 256)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Length <= maxLength;
        }

        public static bool IsValidPhone(string input)
        {
            var phoneRegex = new Regex(@"^\+?\d{7,15}$", RegexOptions.Compiled);
            return phoneRegex.IsMatch(input);
        }
    }
}
