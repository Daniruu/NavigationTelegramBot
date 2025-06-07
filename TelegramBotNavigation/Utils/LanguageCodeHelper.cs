using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Utils
{
    public static class LanguageCodeHelper
    {
        public static LanguageCode FromTelegramTag(string? code)
        {
            return code?.ToLower() switch
            {
                "ru" => LanguageCode.Ru,
                "en" => LanguageCode.En,
                "pl" => LanguageCode.Pl,
                "tr" => LanguageCode.Tr,
                _ => LanguageCode.En
            };
        }

        public static string ToLanguageTag(this LanguageCode code)
        {
            return code.ToString().ToLower();
        }

        public static string GetDisplayLabel(this LanguageCode code)
        {
            return code switch
            {
                LanguageCode.Tr => "🇹🇷 Türkçe",
                LanguageCode.En => "🇬🇧 English",
                LanguageCode.Ru => "🇷🇺 Русский",
                LanguageCode.Pl => "🇵🇱 Polski",
                _ => code.ToString()
            };
        }
    }
}
