using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class WelcomeMessage
    {
        public int Id { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public string Text { get; set; } = string.Empty;

        public string? ImageFileId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
