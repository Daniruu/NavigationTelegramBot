using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.CommandHandlers.Admin
{
    public class AdminCommandHandler : ICommandHandler
    {
        public string Command => "/admin";

        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<AdminCommandHandler> _logger;

        public AdminCommandHandler(
            IUserService userService,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<AdminCommandHandler> logger)
        {
            _userService = userService;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
        }
        public async Task HandleAsync(Message message, string[] args, CancellationToken ct)
        {
            if (message.Chat.Type != ChatType.Private) return;

            var chatId = message.Chat.Id;
            var userId = message.From!.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("Access denied for user {UserId} when attempting to execute an admin command: {command}.", userId, Command);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            var template = await AdminPanelMainTemplate.CreateAsync(user.LanguageCode, _localizer);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
