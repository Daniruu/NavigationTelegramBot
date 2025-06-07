using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserRole?> GetRoleAsync(long id);
        Task<bool> IsAdminAsync(long id);
        bool IsAdmin(TelegramUser user);
        Task<bool> SetRoleAsync(long id, UserRole newRole);
    }
}
