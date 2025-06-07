using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class TelegramUser
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public long ChatId { get; set; }
        public bool IsBlocked { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public DateTime LastActiveAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
