using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.DTOs
{
    public class UserDetailsDto
    {
        public long TelegramUserId { get; set; }
        public string? Username { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public bool IsBlocked { get; set; }
        public long ChatId { get; set; }
        public DateTime LastActionTime { get; set; }
        public int TotalActions { get; set; }
    }
}
