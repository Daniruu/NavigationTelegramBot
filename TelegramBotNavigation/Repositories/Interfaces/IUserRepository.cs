using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<TelegramUser?> GetByIdAsync(long telegramId);
        Task<IEnumerable<TelegramUser>> GetAllAsync();
        Task<PaginatedUserListDto> GetPaginatedRecentUsersAsync(int page, int pageSize = 20);
        Task AddAsync(TelegramUser user);
        Task UpdateAsync(TelegramUser user);
        Task DeleteAsync(TelegramUser user);
    }
}
