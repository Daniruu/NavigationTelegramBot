using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface ISupportRequestRepository
    {
        Task<SupportRequest?> GetByIdAsync(int id);
        Task<List<SupportRequest>> GetOpenRequestsAsync();
        Task AddAsync(SupportRequest request);
        Task SaveChangesAsync();
        Task<SupportRequest?> GetByTopicIdAsync(int topicId);
    }
}
