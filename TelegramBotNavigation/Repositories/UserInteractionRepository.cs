using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class UserInteractionRepository : IUserInteractionRepository
    {
        private readonly ApplicationDbContext _context;

        public UserInteractionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserInteraction interaction)
        {
            await _context.UserInteractions.AddAsync(interaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserInteraction>> GetLatestAsync(int count = 100)
        {
            return await _context.UserInteractions
                .OrderByDescending(i => i.TimeStamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<UserInteraction>> GetByUserIdAsync(long telegramUserId)
        {
            return await _context.UserInteractions
                .Where(i => i.TelegramUserId == telegramUserId)
                .OrderByDescending(i => i.TimeStamp)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetInteractionStatsAsync(ActionType type, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.UserInteractions
                .Where(i => i.ActionType == type);

            if (from.HasValue)
                query = query.Where(i => i.TimeStamp >= from.Value);
            if (to.HasValue)
                query = query.Where(i => i.TimeStamp <= to.Value);

            return await query
                .GroupBy(i => i.Value)
                .OrderByDescending(g => g.Count())
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<int> CountAsync(ActionType type, DateTime? from = null, DateTime? to = null)
        {
            var query = _context.UserInteractions
            .Where(i => i.ActionType == type);

            if (from.HasValue)
                query = query.Where(i => i.TimeStamp >= from.Value);
            if (to.HasValue)
                query = query.Where(i => i.TimeStamp <= to.Value);

            return await query.CountAsync();
        }
    }
}
