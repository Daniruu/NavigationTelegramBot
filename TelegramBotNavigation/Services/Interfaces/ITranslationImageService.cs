using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ITranslationImageService
    {
        Task<string?> GetTranslationImageAsync(string key, LanguageCode languageCode);
        Task SetTranslationImageAsync(string key, LanguageCode languageCode, string fileId);
        Task DeleteTranslationImageAsync(string key, LanguageCode languageCode);
    }
}
