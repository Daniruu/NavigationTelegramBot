using Telegram.Bot.Types;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Services
{
    public class UserInteractionService : IUserInteractionService
    {
        private readonly IUserInteractionRepository _interactionRepository;

        public UserInteractionService(IUserInteractionRepository interactionRepository)
        {
            _interactionRepository = interactionRepository;
        }

        public async Task LogAsync(User user, long chatId, ActionType actionType, string value)
        {
            var interaction = new UserInteraction
            {
                TelegramUserId = user.Id,
                ActionType = actionType,
                Value = value,
                TimeStamp = DateTime.UtcNow,
                LanguageCode = LanguageCodeHelper.FromTelegramTag(user.LanguageCode),
                ChatId = chatId
            };

            await _interactionRepository.AddAsync(interaction);
        }

        public async Task<List<UserInteraction>> GetLatestAsync(int count = 100)
        {
            return await _interactionRepository.GetLatestAsync(count);
        }

        public async Task<List<UserInteraction>> GetByUserIdAsync(long telegramUserId)
        {
            return await _interactionRepository.GetByUserIdAsync(telegramUserId);
        }

        public async Task<Dictionary<string, int>> GetInteractionStatsAsync(ActionType type, DateTime? from = null, DateTime? to = null)
        {
            return await _interactionRepository.GetInteractionStatsAsync(type, from, to);
        }

        public async Task<int> CountAsync(ActionType type, DateTime? from = null, DateTime? to = null)
        {
            return await _interactionRepository.CountAsync(type, from, to);
        }
    }
}
