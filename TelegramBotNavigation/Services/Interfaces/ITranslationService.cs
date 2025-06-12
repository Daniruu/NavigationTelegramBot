using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ITranslationService
    {
        Task<string?> GetTranslationAsync(string key, LanguageCode languageCode);
        Task SetTranslationAsync(string key, LanguageCode languageCode, string value);
        Task DeleteTranslationAsync(string key, LanguageCode languageCode);
    }
}
