using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotNavigation.Bot.Templates
{
    public class TelegramTemplate
    {
        /// <summary>
        /// Основной текст сообщения
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Inline клавиатура (для CallbackQuery)
        /// </summary>
        public InlineKeyboardMarkup? InlineMarkup { get; set; } = null;

        /// <summary>
        /// Reply клавиатура (reply-кнопки, например "Back", "Language")
        /// </summary>
        public ReplyKeyboardMarkup? ReplyMarkup { get; set; } = null;

        /// <summary>
        /// Удаляет reply-клавиатуру (если true — добавляется ReplyKeyboardRemove)
        /// </summary>
        public bool RemoveReplyKeyboard { get; set; } = false;

        /// <summary>
        /// Режим форматирования текста (по умолчанию HTML)
        /// </summary>
        public ParseMode ParseMode { get; set; } = ParseMode.Html;

        /// <summary>
        /// URL или file_id изображения (если не null, будет отправлено фото вместо текста)
        /// </summary>
        public string? Photo { get; set; }

        /// <summary>
        /// Должно ли быть отправлено как уведомление (по умолчанию false)
        /// </summary>
        public bool DisableNotification { get; set; } = false;

        public bool IsPhotoMessage => !string.IsNullOrWhiteSpace(Photo);

        /// <summary>
        /// Объект для генерации ответа с минимальной конфигурацией
        /// </summary>
        public static TelegramTemplate Create(
            string text,
            InlineKeyboardMarkup? inline = null,
            ReplyKeyboardMarkup? reply = null,
            bool removeReplyKeyboard = false,
            string? photo = null)
        {
            return new TelegramTemplate
            {
                Text = text,
                InlineMarkup = inline,
                ReplyMarkup = reply,
                RemoveReplyKeyboard = removeReplyKeyboard,
                Photo = photo
            };
        }

    }
}
