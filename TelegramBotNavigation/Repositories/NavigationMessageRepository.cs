using Microsoft.EntityFrameworkCore;
using System;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class NavigationMessageRepository : INavigationMessageRepository
    {
        private readonly ApplicationDbContext _context;

        public NavigationMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(NavigationMessage message)
        {
            await _context.NavigationMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(NavigationMessage message)
        {
            _context.NavigationMessages.Update(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByIdAsync(int id)
        {
            var message = await _context.NavigationMessages.FindAsync(id);
            if (message != null)
            {
                _context.NavigationMessages.Remove(message);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<NavigationMessage>> GetByChatIdAsync(long chatId)
        {
            return await _context.NavigationMessages
                .Where(m => m.ChatId == chatId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NavigationMessage>> GetAllAsync()
        {
            return await _context.NavigationMessages.ToListAsync();
        }
    }
}
