using Telegram.Bot.Types;

namespace TelegramBotNavigation.Bot
{
    public interface IUpdateProcessor
    {
        Task HandleAsync(Update update, CancellationToken ct);
    }
}
