using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly ApplicationDbContext _context;

        public MenuItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MenuItem?> GetByIdAsync(int id)
        {
            return await _context.MenuItems
                .Include(mi => mi.SubMenu)
                .FirstOrDefaultAsync(mi => mi.Id == id);
        }
    }
}
