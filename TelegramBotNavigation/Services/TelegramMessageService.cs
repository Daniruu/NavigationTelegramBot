using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class TelegramMessageService : ITelegramMessageService
    {
        private readonly ITelegramBotClient _bot;
        private readonly ILogger<TelegramMessageService> _logger;

        private readonly Dictionary<(long ChatId, int MessageId), TelegramTemplate> _editCache = new();

        public TelegramMessageService(ITelegramBotClient bot, ILogger<TelegramMessageService> logger)
        {
            _bot = bot;
            _logger = logger;
        }
        public async Task<Message> SendTemplateAsync(long chatId, TelegramTemplate template, CancellationToken cancellationToken)
        {
            ReplyMarkup? markup = template.ReplyMarkup
                ?? (ReplyMarkup?)template.InlineMarkup
                ?? (template.RemoveReplyKeyboard ? new ReplyKeyboardRemove() : null);

            if (template.IsPhotoMessage)
            {
                return await _bot.SendPhoto(
                    chatId: chatId,
                    photo: template.Photo!,
                    caption: template.Text,
                    parseMode: template.ParseMode,
                    replyMarkup: markup,
                    disableNotification: template.DisableNotification,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                return await _bot.SendMessage(
                    chatId: chatId,
                    text: template.Text,
                    parseMode: template.ParseMode,
                    replyMarkup: markup,
                    disableNotification: template.DisableNotification,
                    cancellationToken: cancellationToken
                );
            }
        }

        public async Task<Message> SendTemplateAsync(string channelUsername, TelegramTemplate template, CancellationToken cancellationToken)
        {
            ReplyMarkup? markup = template.ReplyMarkup
                ?? (ReplyMarkup?)template.InlineMarkup
                ?? (template.RemoveReplyKeyboard ? new ReplyKeyboardRemove() : null);

            if (template.IsPhotoMessage)
            {
                return await _bot.SendPhoto(
                    chatId: channelUsername,
                    photo: template.Photo,
                    caption: template.Text,
                    parseMode: template.ParseMode,
                    replyMarkup: markup,
                    disableNotification: template.DisableNotification,
                    cancellationToken: cancellationToken
                );
            }
            else
            {
                return await _bot.SendMessage(
                    chatId: channelUsername,
                    text: template.Text,
                    parseMode: template.ParseMode,
                    replyMarkup: markup,
                    disableNotification: template.DisableNotification,
                    cancellationToken: cancellationToken
                );
            }
        }

        public async Task EditTemplateAsync(long chatId, int messageId, TelegramTemplate template, CancellationToken cancellationToken)
        {
            ReplyMarkup? markup = template.ReplyMarkup
                    ?? (ReplyMarkup?)template.InlineMarkup
                    ?? (template.RemoveReplyKeyboard ? new ReplyKeyboardRemove() : null);

            try
            {
                // Фото
                if (template.IsPhotoMessage)
                {
                    // Замена самой картинки — если необходимо
                    var media = new InputMediaPhoto(template.Photo!)
                    {
                        Caption = template.Text,
                        ParseMode = template.ParseMode
                    };

                    await _bot.EditMessageMedia(
                        chatId: chatId,
                        messageId: messageId,
                        media: media,
                        replyMarkup: template.InlineMarkup,
                        cancellationToken: cancellationToken
                    );
                }
                // Текст
                else
                {
                    await _bot.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: template.Text,
                        parseMode: template.ParseMode,
                        replyMarkup: template.InlineMarkup,
                        cancellationToken: cancellationToken
                    );
                }

                _editCache[(chatId, messageId)] = template;
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                if (ex.Message.Contains("there is no text in the message to edit")) //  || ex.Message.Contains("message is not modified")
                {
                    _logger.LogWarning("Edit failed: {Message}. Deleting and sending new message.", ex.Message);

                    await _bot.DeleteMessage(chatId, messageId, cancellationToken);

                    if (template.IsPhotoMessage)
                    {
                        await _bot.SendPhoto(
                            chatId: chatId,
                            photo: template.Photo!,
                            caption: template.Text,
                            parseMode: template.ParseMode,
                            replyMarkup: markup,
                            disableNotification: template.DisableNotification,
                            cancellationToken: cancellationToken
                        );
                    }
                    else
                    {
                        await _bot.SendMessage(
                            chatId: chatId,
                            text: template.Text,
                            parseMode: template.ParseMode,
                            replyMarkup: markup,
                            disableNotification: template.DisableNotification,
                            cancellationToken: cancellationToken
                        );
                    }
                }
                else
                {
                    _logger.LogError(ex, "Failed to edit message in chat {ChatId}, message {MessageId}", chatId, messageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to edit message in chat {ChatId}, message {MessageId}", chatId, messageId);
            }
        }

    }
}
