namespace TelegramBotNavigation.DTOs
{
    public class UserSummaryDto
    {
        public long TelegramUserId { get; set; }
        public string? Username { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public DateTime LastActionTime { get; set; }
    }
}
