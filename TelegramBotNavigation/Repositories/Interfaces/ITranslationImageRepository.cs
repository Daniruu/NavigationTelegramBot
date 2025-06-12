using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ITranslationImageRepository
    {
        Task<string?> GetFileIdAsync(string key, LanguageCode languageCode);
        Task<TranslationImage?> GetAsync(string key, LanguageCode languageCode);
        Task AddAsync(TranslationImage translationImage);
        Task UpdateAsync(TranslationImage translationImage);
        Task DeleteAsync(string key, LanguageCode languageCode);
    }
}
