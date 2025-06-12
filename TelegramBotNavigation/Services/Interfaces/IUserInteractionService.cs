using Telegram.Bot.Types;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Services.Interfaces
{
    public interface IUserInteractionService
    {
        Task LogAsync(TelegramUser user, long chatId, ActionType actionType, string value);
        Task<List<UserInteraction>> GetLatestAsync(int count = 100);
        Task<List<UserInteraction>> GetByUserIdAsync(long telegramUserId);
        Task<Dictionary<string, int>> GetInteractionStatsAsync(ActionType actionType, DateTime? from = null, DateTime? to = null);
        Task<int> CountAsync(ActionType actionType, DateTime? from = null, DateTime? to = null);
    }
}
