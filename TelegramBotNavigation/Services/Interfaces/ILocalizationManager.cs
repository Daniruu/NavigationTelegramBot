using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ILocalizationManager
    {
        Task<string> GetInterfaceTranslation(string key, LanguageCode languageCode, params object[] args);
        Task<string?> GetCustomTranslationAsync(string key, LanguageCode userLang, params object[] args);
        Task<string?> GetImageTranslationAsync(string key, LanguageCode userLang);
    }
}
