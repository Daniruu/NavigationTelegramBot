using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Templates;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ITelegramMessageService
    {
        Task<Message> SendTemplateAsync(long chatId, TelegramTemplate template, CancellationToken cancellationToken);
        Task<Message> SendTemplateAsync(string channelUsername, TelegramTemplate template, CancellationToken cancellationToken);
        Task EditTemplateAsync(long chatId, int messageId, TelegramTemplate template, CancellationToken cancellationToken);
    }
}
