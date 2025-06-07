using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public string LabelTranslationKey { get; set; } = string.Empty;

        public MenuActionType ActionType { get; set; } = MenuActionType.Url;

        public string? Url { get; set; } = string.Empty;
        public string? MessageTranslationKey { get; set; } = string.Empty;
        public int? SubMenuId { get; set; } = null;

        public int Row { get; set; }    // Строка в меню
        public int Order { get; set; }  // Порядок в строке

        public Menu Menu { get; set; } = null!;
        public Menu? SubMenu { get; set; } = null;
    }
}
