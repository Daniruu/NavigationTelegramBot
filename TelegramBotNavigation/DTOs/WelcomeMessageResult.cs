namespace TelegramBotNavigation.DTOs
{
    public class WelcomeMessageResult
    {
        public WelcomeMessageDto Message { get; set; } = null!;
        public bool IsCustom { get; set; }
    }
}
