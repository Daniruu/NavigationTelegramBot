using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ILanguageSettingRepository
    {
        Task<List<LanguageCode>> GetFallbackOrderAsync();
        Task<LanguageSetting?> GetByIdAsync(int id);
        Task<List<LanguageSetting>> GetLanguageSettingsAsync();
        Task UpdateAsync(LanguageSetting languageSetting);
    }
}
