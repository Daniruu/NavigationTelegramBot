using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotNavigation.Bot
{
    public interface ITelegramClient
    {
        Task<int> CreateForumTopicAsync(long chatId, string topicName, string? emoji = null, CancellationToken ct = default);
        Task EditForumTopicIconAsync(long chatId, int messageThreadId, string emoji, CancellationToken ct = default);
        Task PinMessageAsync(long chatId, int messageId, bool disableNotification = true, CancellationToken ct = default);
    }
}
