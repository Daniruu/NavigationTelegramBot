using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public class MenuRepository : IMenuRepository
    {
        private readonly ApplicationDbContext _context;

        public MenuRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Menu?> GetByIdAsync(int id)
        {
            return await _context.Menus
                .Include(m => m.MenuItems)
                .Include(m => m.ParentMenu)
                    .ThenInclude(pm => pm.MenuItems)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Menu>> GetTopLevelMenusAsync()
        {
            return await _context.Menus.Where(m => m.ParentMenuId == null)
                                       .Include(m => m.MenuItems)
                                       .ToListAsync();
        }

        public async Task<IEnumerable<Menu>> GetByParentIdAsync(int parentId)
        {
            return await _context.Menus.Where(m => m.ParentMenuId == parentId)
                                        .Include(m => m.MenuItems)
                                        .ToListAsync();
        }

        public async Task<IEnumerable<Menu>> GetAllAsync()
        {
            return await _context.Menus.Include(m => m.MenuItems).ToListAsync();
        }

        public async Task<Menu?> GetMainMenuAsync()
        {
            return await _context.Menus.Include(m => m.MenuItems)
                                       .FirstOrDefaultAsync(m => m.IsMainMenu);
        }

        public async Task AddAsync(Menu menu)
        {
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Menu menu)
        {
            _context.Menus.Update(menu);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Menu menu)
        {
            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
