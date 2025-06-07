using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Repositories.Interfaces
{
    public interface IUserInteractionRepository
    {
        /// <summary>
        /// Добавляет запись о действии пользователя.
        /// </summary>
        Task AddAsync(UserInteraction interaction);

        /// <summary>
        /// Возвращает последние N действий.
        /// </summary>
        Task<List<UserInteraction>> GetLatestAsync(int count = 100);

        /// <summary>
        /// Возвращает все действия пользователя по его Telegram ID.
        /// </summary>
        Task<List<UserInteraction>> GetByUserIdAsync(long telegramUserId);

        /// <summary>
        /// Возвращает сгруппированную статистику по Value (например, для аналитики интересов).
        /// </summary>
        Task<Dictionary<string, int>> GetInteractionStatsAsync(ActionType type, DateTime? from = null, DateTime? to = null);

        /// <summary>
        /// Возвращает количество действий по типу (например, количество команд).
        /// </summary>
        Task<int> CountAsync(ActionType type, DateTime? from = null, DateTime? to = null);
    }

}
