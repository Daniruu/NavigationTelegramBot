using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class SupportRequest
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public TelegramUser User { get; set; } = null!;

        public SupportStatus Status { get; set; } = SupportStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ClosedAt { get; set; } = DateTime.MaxValue;

        /// <summary>
        /// ID темы в супергруппе Telegram (topic_id)
        /// </summary>
        public int? TopicId { get; set; }

        /// <summary>
        /// ID сообщения (в супергруппе), которое отображает первую отправку запроса
        /// </summary>
        public int? AdminMessageId { get; set; }

        /// <summary>
        /// Список сообщений в рамках этого запроса
        /// </summary>
        public List<SupportMessage> Messages { get; set; } = new();
    }
}
