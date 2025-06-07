using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface IWelcomeMessageRepository
    {
        Task<WelcomeMessage?> GetByLanguageCodeAsync(LanguageCode languageCode);
        Task SetAsync(LanguageCode languageCode, string? text, string? imageFileId);
        Task RemoveImageAsync(LanguageCode languageCode);
    }
}
