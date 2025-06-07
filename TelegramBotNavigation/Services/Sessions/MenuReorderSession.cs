namespace TelegramBotNavigation.Services.Sessions
{
    public class MenuReorderSession
    {
        public int MenuId { get; set; }
        public int SelectedItemId { get; set; }
        public List<MenuItemSnapshot> Items { get; set; } = new();
    }
}
