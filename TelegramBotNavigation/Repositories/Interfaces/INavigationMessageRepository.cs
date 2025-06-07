using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface INavigationMessageRepository
    {
        Task AddAsync(NavigationMessage message);
        Task UpdateAsync(NavigationMessage message);
        Task DeleteByIdAsync(int id);
        Task<IEnumerable<NavigationMessage>> GetByChatIdAsync(long chatId);
        Task<IEnumerable<NavigationMessage>> GetAllAsync();
    }
}
