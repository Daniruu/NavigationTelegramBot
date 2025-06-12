using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class SupportRequestRepository : ISupportRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public SupportRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SupportRequest?> GetByIdAsync(int id)
        {
            return await _context.SupportRequests
                .Include(r => r.User)
                .Include(r => r.Messages).OrderBy(m => m.CreatedAt)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<SupportRequest>> GetOpenRequestsAsync()
        {
            return await _context.SupportRequests
                .Where(r => r.Status != SupportStatus.Resolved)
                .Include(r => r.Messages).OrderBy(m => m.CreatedAt)
                .Include(r => r.User)
                .ToListAsync();
        }

        public async Task AddAsync(SupportRequest request)
        {
            await _context.SupportRequests.AddAsync(request);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<SupportRequest?> GetByTopicIdAsync(int topicId)
        {
            return await _context.SupportRequests
                .Include(r => r.User)
                .Include(r => r.Messages).OrderBy(m => m.CreatedAt)
                .FirstOrDefaultAsync(r => r.TopicId == topicId);
        }
    }
}
