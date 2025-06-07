using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class Translation
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
