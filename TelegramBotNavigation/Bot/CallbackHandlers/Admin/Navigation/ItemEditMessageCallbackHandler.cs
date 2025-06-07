using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation
{
    public class ItemEditMessageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.ItemEditMessage;

        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILogger<ItemEditMessageCallbackHandler> _logger;
        private readonly IMenuRepository _menuRepository;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly ISessionManager _sessionManager;

        public ItemEditMessageCallbackHandler(
            IUserRepository userRepository,
            IUserService userService, ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILogger<ItemEditMessageCallbackHandler> logger,
            IMenuRepository menuRepository,
            ILanguageSettingRepository languageSettingRepository,
            ISessionManager sessionManager)
        {
            _userRepository = userRepository;
            _userService = userService;
            _messageService = messageService;
            _localizer = localizer;
            _logger = logger;
            _menuRepository = menuRepository;
            _languageSettingRepository = languageSettingRepository;
            _sessionManager = sessionManager;
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

            if (args.Length < 2) return;

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

            var menuItemIdStr = args[1];
            if (!int.TryParse(menuItemIdStr, out var menuItemId))
            {
                _logger.LogWarning("Invalid menuItemId format: {menuItemIdStr}", menuItemIdStr);
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidMenuItemId, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var itemToEdit = menu.MenuItems.FirstOrDefault(i => i.Id == menuItemId);
            if (itemToEdit == null) return;

            string selectedLang;

            if (args.Length < 3)
            {
                var fallbackOrder = await _languageSettingRepository.GetFallbackOrderAsync();
                selectedLang = fallbackOrder.First().ToLanguageTag();
            }
            else
            {
                selectedLang = args[2];
            }

            await _sessionManager.ClearSessionAsync(userId);

            await _sessionManager.SetSessionAsync(userId, new SessionData
            {
                Action = SessionKeys.NavigationItemEditMessage,
                Data = new Dictionary<string, string>
                {

                    { "menuId", menuId.ToString() },
                    { "menuItemId", menuItemId.ToString() },
                    { "lang", selectedLang },
                }
            }, TimeSpan.FromMinutes(10));

            var languageCode = LanguageCodeHelper.FromTelegramTag(selectedLang);

            var prompt = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.EnterButtonMessage, user.LanguageCode, languageCode.GetDisplayLabel());
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(prompt), ct);
        }
    }
}
