using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface IWelcomeMessageProvider
    {
        Task<WelcomeMessageDto> GetForUserAsync(LanguageCode userLang);
        Task<WelcomeMessageResult> GetDetailedForAdminAsync(LanguageCode requestedLang);
    }
}
