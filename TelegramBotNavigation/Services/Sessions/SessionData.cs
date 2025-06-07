namespace TelegramBotNavigation.Services.Sessions
{
    public class SessionData
    {
        public string Action { get; set; } = string.Empty;
        public Dictionary<string, string> Data { get; set; } = new();
    }
}
