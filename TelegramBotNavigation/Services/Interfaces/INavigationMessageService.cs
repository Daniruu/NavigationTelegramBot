using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface INavigationMessageService
    {
        Task SendNavigationAsync(long chatId, LanguageCode languageCode, CancellationToken ct);
        Task UpdateAllNavigationMessagesAsync(CancellationToken ct);
        Task UpdateByChatIdAsync(long chatId, LanguageCode languageCode, CancellationToken ct);
    }
}
