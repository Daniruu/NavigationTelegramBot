using Telegram.Bot.Types;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public interface ISessionHandler
    {
        string Action { get; }
        Task HandleAsync(Message message, SessionData session, CancellationToken ct);
    }
}
