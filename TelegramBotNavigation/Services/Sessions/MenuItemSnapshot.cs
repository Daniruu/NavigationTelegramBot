namespace TelegramBotNavigation.Services.Sessions
{
    public class MenuItemSnapshot
    {
        public int Id { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Row { get; set; }
        public int Order { get; set; }
    }
}
