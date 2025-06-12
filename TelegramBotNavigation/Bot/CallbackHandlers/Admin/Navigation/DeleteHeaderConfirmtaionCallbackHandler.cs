using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class DeleteHeaderConfirmtaionCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.DeleteHeaderConfirmation;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<DeleteHeaderConfirmtaionCallbackHandler> _logger;
        private readonly IMenuRepository _menuRepository;
        private readonly ICallbackAlertService _callbackAlertService;

        public DeleteHeaderConfirmtaionCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<DeleteHeaderConfirmtaionCallbackHandler> logger,
            IMenuRepository menuRepository,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _menuRepository = menuRepository;
            _callbackAlertService = callbackAlertService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
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

            if (args.Length < 2) return;

            var menuIdStr = args[0];
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.InvalidMenuId, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(Errors.MenuNotFound, user.LanguageCode);
                await _callbackAlertService.ShowAsync(query.Id, errorMessage, cancellationToken: ct);
                return;
            }

            LanguageCode languageCode = LanguageCodeHelper.FromTelegramTag(args[1]);

            var template = await DeleteHeaderConfirmationTemplate.CreateAsync(menu.Id, user.LanguageCode, languageCode, _localizer);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
