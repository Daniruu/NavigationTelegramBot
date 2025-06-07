using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class LanguageSetting
    {
        public int Id { get; set; }
        public LanguageCode LanguageCode { get; set; }
        public int Priority { get; set; }
    }
}
