using Telegram.Bot.Types;

namespace TelegramBotNavigation.Bot.MessageHandlers
{
    public interface IMessageHandler
    {
        bool CanHandle(Message message);
        Task HandleAsync(Message message, CancellationToken ct);
    }
}
