using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates.Admin;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Utils;
using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ItemAddCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.NavigationItemAdd;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ILogger<ItemAddCallbackHandler> _logger;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly ISessionManager _sessionManager;
        private readonly ILanguageSettingRepository _languageSettingRepository;

        public ItemAddCallbackHandler(
            IUserRepository userRepository,
            IUserService userService,
            ILogger<ItemAddCallbackHandler> logger,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            ISessionManager sessionManager,
            ILanguageSettingRepository languageSettingRepository)
        {
            _userRepository = userRepository;
            _userService = userService;
            _logger = logger;
            _messageService = messageService;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _sessionManager = sessionManager;
            _languageSettingRepository = languageSettingRepository;
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
                _logger.LogWarning("User {UserId} not found when trying to access admin command.", userId);
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
            string selectedLang;

            if (args.Length > 1)
            {
                selectedLang = args[1];
            }
            else
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                selectedLang = fallbackOrder.First().ToLanguageTag();
            }

            await _sessionManager.ClearSessionAsync(userId);

            await _sessionManager.SetSessionAsync(userId, new SessionData
            {
                Action = SessionKeys.NavigationItemSelectType,
                Data = new Dictionary<string, string>
                {
                    { "menuId", menuId.ToString() },
                    { "lang", selectedLang }
                }
            }, TimeSpan.FromMinutes(10));

            LanguageCode languageCode = LanguageCodeHelper.FromTelegramTag(selectedLang);

            var template = await AddItemTypeSelectionTemplate.CreateAsync(user.LanguageCode, languageCode, _localizer, _languageSettingRepository, menu);
            await _messageService.EditTemplateAsync(chatId, messageId, template, ct);
        }
    }
}
