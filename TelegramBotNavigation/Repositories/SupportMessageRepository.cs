using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class SupportMessageRepository : ISupportMessageRepository
    {
        private readonly ApplicationDbContext _context;
        public SupportMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SupportMessage message)
        {
            await _context.SupportMessages.AddAsync(message);
        }

        public async Task<List<SupportMessage>> GetMessagesByRequestIdAsync(int requestId)
        {
            return await _context.SupportMessages
                .Where(m => m.SupportRequestId == requestId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
