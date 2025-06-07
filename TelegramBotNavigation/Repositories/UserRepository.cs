using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TelegramUser?> GetByIdAsync(long telegramId)
        {
            return await _context.TelegramUsers.FindAsync(telegramId);
        }

        public async Task<PaginatedUserListDto> GetPaginatedRecentUsersAsync(int page, int pageSize = 20)
        {
            var query = _context.TelegramUsers.OrderByDescending(u => u.LastActiveAt);

            var total = await query.CountAsync();

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserSummaryDto
                {
                    TelegramUserId = u.Id,
                    Username = u.Username,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    LastActionTime = u.LastActiveAt,
                    //TotalActions = _context.UserInteractions.Count(i => i.TelegramUserId == u.Id)
                })
                .ToListAsync();

            return new PaginatedUserListDto
            {
                Users = users,
                TotalPages = (int)Math.Ceiling((double)total / pageSize),
                CurrentPage = page
            };
        }

        public async Task<IEnumerable<TelegramUser>> GetAllAsync()
        {
            return await _context.TelegramUsers.ToListAsync();
        }

        public async Task AddAsync(TelegramUser user)
        {
            _context.TelegramUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TelegramUser user)
        {
            _context.TelegramUsers.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TelegramUser user)
        {
            _context.TelegramUsers.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
