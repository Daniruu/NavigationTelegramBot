namespace TelegramBotNavigation.Models
{
    public class TranslationImage
    {
        public int Id { get; set; }
        public string Key { get; set; } = null!;
        public string Language { get; set; } = null!;

        public string FileId { get; set; } = null!;
        public string? Type { get; set; }
    }
}
