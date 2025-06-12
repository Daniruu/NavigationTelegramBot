using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CommandHandlers.Common
{
    public class StartCommandHandler : ICommandHandler
    {
        public string Command => "/start";

        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ICommandSetupService _commandSetupService;
        private readonly IWelcomeMessageProvider _welcomeMessageProvider;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly IUserInteractionService _userInteractionService;

        public StartCommandHandler(
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ICommandSetupService commandSetupService,
            IWelcomeMessageProvider welcomeMessageProvider,
            INavigationMessageService navigationMessageService,
            IUserInteractionService userInteractionService
            )
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _commandSetupService = commandSetupService;
            _welcomeMessageProvider = welcomeMessageProvider;
            _navigationMessageService = navigationMessageService;
            _userInteractionService = userInteractionService;
        }

        public async Task HandleAsync(Message message, string[] args, CancellationToken ct)
        {
            if (message.Chat.Type != ChatType.Private) return;

            var userId = message.From!.Id;
            var languageCode = message.From.LanguageCode;
            var chatId = message.Chat.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                user = new TelegramUser
                {
                    Id = userId,
                    Username = message.From!.Username,
                    FirstName = message.From!.FirstName,
                    LastName = message.From!.LastName,
                    LanguageCode = LanguageCodeHelper.FromTelegramTag(languageCode),
                    CreatedAt = DateTime.UtcNow,
                    LastActiveAt = DateTime.UtcNow,
                    ChatId = chatId,
                };

                await _userRepository.AddAsync(user);
            }
            else
            {
                user.Username = message.From!.Username;
                user.FirstName = message.From!.FirstName;
                user.LastName = message.From!.LastName;
                user.LastActiveAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
            }

            await _commandSetupService.SetupCommandsAsync(chatId, user, ct);

            await _userInteractionService.LogAsync(user, chatId, Enums.ActionType.Command, Command);

            var welcomeTemplate = await StartTemplate.CreateAsync(user.LanguageCode, _welcomeMessageProvider);
            await _messageService.SendTemplateAsync(chatId, welcomeTemplate, ct);

            await _navigationMessageService.SendNavigationAsync(chatId, user.LanguageCode, ct);
        }
    }
}
