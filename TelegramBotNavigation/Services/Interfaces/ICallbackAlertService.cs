namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ICallbackAlertService
    {
        Task ShowAsync(string CallbackQueryId, string message, bool showAlert = false, string? url = null, CancellationToken cancellationToken = default);
    }
}
