using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ILanguageSettingRepository
    {
        Task<List<LanguageCode>> GetFallbackOrderAsync();
    }
}
