using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Settings
{
    public class SetAdminCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.SetAdmin;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<SetAdminCallbackHandler> _logger;
        private readonly ILocalizationManager _localizer;
        private readonly ICallbackAlertService _callbackAlertService;
        private readonly ICommandSetupService _commandSetupService;
        private readonly ITelegramMessageService _messageService;

        public SetAdminCallbackHandler(
            IUserRepository userRepository, 
            IUserService userService, 
            ILogger<SetAdminCallbackHandler> logger, 
            ILocalizationManager localizer, 
            ICallbackAlertService callbackAlertService,
            ICommandSetupService commandSetupService,
            ITelegramMessageService messageService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _localizer = localizer;
            _callbackAlertService = callbackAlertService;
            _commandSetupService = commandSetupService;
            _messageService = messageService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            if (args.Length == 0 || !long.TryParse(args[0], out var targetUserId)) return;

            var chatId = query.Message!.Chat.Id;
            var messageId = query.Message.MessageId;
            var userId = query.From.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (!_userService.IsAdmin(user))
            {
                _logger.LogWarning("Access denied for user {UserId} while trying to access admin callback.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.NotAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, showAlert: true, cancellationToken: ct);
                return;
            }

            var targetUser = await _userRepository.GetByIdAsync(targetUserId);
            if (targetUser == null) return;
            if (_userService.IsAdmin(targetUser))
            {
                var _errorMessage = await _localizer.GetInterfaceTranslation(Errors.AlreadyAdmin, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, _errorMessage, cancellationToken: ct);
                return;
            }

            await _userService.SetRoleAsync(targetUserId, Enums.UserRole.Admin);

            await _commandSetupService.SetupCommandsAsync(targetUserId, targetUser, ct);

            var successMessage = await _localizer.GetInterfaceTranslation(Notifications.AdminSet, user.LanguageCode);
            await _callbackAlertService.ShowAsync(query.Id, successMessage, showAlert: true, cancellationToken: ct);

            var temlate = await UnsetAdminTemplate.CreateAsync(user.LanguageCode, _localizer, targetUser);
            await _messageService.EditTemplateAsync(chatId, messageId, temlate, ct);
        }
    }
}
