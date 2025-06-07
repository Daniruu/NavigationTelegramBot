using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;

        public string HeaderTranslationKey { get; set; } = string.Empty;
        public string HeaderImageTranslationKey { get; set; } = string.Empty;

        public bool IsMainMenu { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        public int? ParentMenuId { get; set; }
        public Menu? ParentMenu { get; set; }
    }
}
