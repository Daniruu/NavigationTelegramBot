using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface IMenuRepository
    {
        Task<Menu?> GetByIdAsync(int id);
        Task<IEnumerable<Menu>> GetAllAsync();
        Task<IEnumerable<Menu>> GetTopLevelMenusAsync();
        Task<IEnumerable<Menu>> GetByParentIdAsync(int parentId);
        Task<Menu?> GetMainMenuAsync();
        Task AddAsync(Menu menu);
        Task UpdateAsync(Menu menu);
        Task DeleteAsync(Menu menu);
        Task SaveChangesAsync();
    }
}
