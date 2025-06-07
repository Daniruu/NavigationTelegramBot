using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ICommandSetupService
    {
        Task SetupCommandsAsync(long chatId, TelegramUser user, CancellationToken ct);
    }
}
