using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ISupportMessageRepository
    {
        Task AddAsync(SupportMessage message);
        Task<List<SupportMessage>> GetMessagesByRequestIdAsync(int requestId);
        Task SaveChangesAsync();
    }
}
