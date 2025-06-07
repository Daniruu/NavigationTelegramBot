using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class NavigationViewCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationView;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NavigationViewCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IMenuButtonBuilder _buttonBuilder;

        public NavigationViewCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<NavigationViewCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository,
            IMenuButtonBuilder buttonBuilder)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _buttonBuilder = buttonBuilder;
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
                _logger.LogWarning("Access denied for user {UserId} when trying to access admin callback.", userId);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NotAdmin, user.LanguageCode);
                var errorTemplate = TelegramTemplate.Create(errorMessage);
                await _messageService.SendTemplateAsync(chatId, errorTemplate, ct);
                return;
            }

            if (args.Length == 0) return;

            var menuIdStr = args[0];
            if (!int.TryParse(menuIdStr, out var menuId))
            {
                _logger.LogWarning("Invalid menuId format: {MenuIdStr}", menuIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var menu = await _menuRepository.GetByIdAsync(menuId);
            if (menu == null)
            {
                var error = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.MenuNotFound, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(error), ct);
                return;
            }

            LanguageCode languageCode;

            if (args.Length < 2)
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                languageCode = fallbackOrder.First();
            }
            else
            {
                var language = args[1];
                languageCode = LanguageCodeHelper.FromTelegramTag(language);
            }

            _logger.LogInformation("Sending navigation view for menu {MenuId} in language {LanguageCode} to user {UserId}.", menuId, languageCode, userId);
            var template = await NavigationViewTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, _buttonBuilder, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
