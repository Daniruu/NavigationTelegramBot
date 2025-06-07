using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ITranslationRepository
    {
        Task<string?> GetValueAsync(string key, LanguageCode languageCode);
        Task<Translation?> GetAsync(string key, LanguageCode languageCode);
        Task AddAsync(Translation translation);
        Task UpdateAsync(Translation translation);
        Task DeleteAsync(string key, LanguageCode languageCode);
        Task DeleteAllByKeyAsync(string key);
    }
}
