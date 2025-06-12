using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface ISupportRequestService
    {
        Task<SupportRequest> CreateRequestAsync(long userId, string message);
        Task AddMessageAsync(int requestId, bool isFromAdmin, string text);
        Task CloseRequestAsync(int requestId);
        Task<SupportRequest?> GetByIdAsync(int requestId);
        Task<SupportRequest?> GetLastOpenRequestForUserAsync(long userId);
        Task SetTelegramTopicAsync(int requestId, int topicId);
        Task SetAdminMessageIdAsync(int requestId, int adminMessageId);
        Task<SupportRequest?> GetRequestByTopicIdAsync(int topicId);
    }
}
