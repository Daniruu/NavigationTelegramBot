using Telegram.Bot.Types;

namespace TelegramBotNavigation.Bot.CallbackHandlers
{
    public interface ICallbackHandler
    {
        string Key { get; }
        Task HandleAsync(CallbackQuery callbackQuery, string[] args, CancellationToken ct);
    }
}
