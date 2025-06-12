using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CommandHandlers.Common
{
    public class NavigationCommandHandler : ICommandHandler
    {
        public string Command => "/navigation";

        private readonly IUserRepository _userRepository;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly IUserInteractionService _userInteractionService;

        public NavigationCommandHandler(IUserRepository userRepository, INavigationMessageService navigationMessageService, IUserInteractionService userInteractionService)
        {
            _userRepository = userRepository;
            _navigationMessageService = navigationMessageService;
            _userInteractionService = userInteractionService;
        }

        public async Task HandleAsync(Message message, string[] args, CancellationToken ct)
        {
            if (message.Chat.Type != ChatType.Private) return;

            var userId = message.From!.Id;
            var chatId = message.Chat.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _userInteractionService.LogAsync(user, chatId, Enums.ActionType.Command, Command);

            await _navigationMessageService.SendNavigationAsync(chatId, user.LanguageCode, ct);
        }
    }
}
