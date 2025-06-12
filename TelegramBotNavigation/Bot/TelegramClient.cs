using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotNavigation.Bot
{
    public class TelegramClient : ITelegramClient
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<TelegramClient> _logger;

        public TelegramClient(ITelegramBotClient bot, ILogger<TelegramClient> logger)
        {
            _bot = bot;
            _logger = logger;
        }

        public async Task<int> CreateForumTopicAsync(long chatId, string topicName, string? selectedEmoji = null, CancellationToken ct = default)
        {
            var icons = await _bot.GetForumTopicIconStickers(ct);
            var emoji = icons.FirstOrDefault(e => e.Emoji == selectedEmoji);
            var emojiId = emoji?.CustomEmojiId;

            try
            {
                var topic = await _bot.CreateForumTopic(
                    chatId: chatId,
                    name: topicName,
                    iconCustomEmojiId: emojiId,
                    cancellationToken: ct
                );

                return topic.MessageThreadId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create support topic '{Topic}' in chat {ChatId}", topicName, chatId);
                throw;
            }
        }

        public async Task EditForumTopicIconAsync(long chatId, int messageThreadId, string selectedEmoji, CancellationToken ct = default)
        {
            var icons = await _bot.GetForumTopicIconStickers(ct);
            var emoji = icons.FirstOrDefault(e => e.Emoji == selectedEmoji);
            var emojiId = emoji?.CustomEmojiId;

            if (emojiId == null)
            {
                _logger.LogWarning("Emoji '{Emoji}' not found for chat {ChatId} and thread {MessageThreadId}", selectedEmoji, chatId, messageThreadId);
                return;
            }

            await _bot.EditForumTopic(
                chatId: chatId,
                messageThreadId: messageThreadId,
                name: null,
                iconCustomEmojiId: emojiId,
                cancellationToken: ct
            );
        }

        public async Task PinMessageAsync(long chatId, int messageId, bool disableNotification = true, CancellationToken ct = default)
        {
            await _bot.PinChatMessage(
                chatId: chatId,
                messageId: messageId,
                disableNotification: disableNotification,
                cancellationToken: ct);
        }
    }
}
