using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Admin;

namespace TelegramBotNavigation.Bot.SessionHandlers
{
    public class NavigationAddSessionHandler : ISessionHandler
    {
        public string Action => SessionKeys.NavigationAdd;

        private readonly IUserRepository _userRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly ILogger<NavigationAddSessionHandler> _logger;
        private readonly ILocalizationManager _localizer;
        private readonly ISessionManager _sessionManager;
        private readonly ITelegramMessageService _messageService;
        private readonly ICallbackAlertService _callbackAlertService;

        public NavigationAddSessionHandler(
            IUserRepository userRepository, 
            IMenuRepository menuRepository, 
            ILogger<NavigationAddSessionHandler> logger, 
            ILocalizationManager localizer, 
            ISessionManager sessionManager,
            ITelegramMessageService messageService)
        {
            _userRepository = userRepository;
            _menuRepository = menuRepository;
            _logger = logger;
            _localizer = localizer;
            _sessionManager = sessionManager;
            _messageService = messageService;
        }

        public async Task HandleAsync(Message message, SessionData session, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var text = message.Text;
            var chatId = message.Chat.Id;
            
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            if (string.IsNullOrEmpty(text))
            {
                var errorMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Errors.InvalidInput, user.LanguageCode);
                await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(errorMessage), ct);
                return;
            }

            var newMenu = new Menu
            {
                Title = text,
                IsMainMenu = false,
                HeaderTranslationKey = $"menu.header.{Guid.NewGuid()}",
                HeaderImageTranslationKey = $"menu.header.image.{Guid.NewGuid()}",
                CreatedAt = DateTime.UtcNow,
            };

            await _menuRepository.AddAsync(newMenu);

            _logger.LogInformation("Navigation menu added, title: {Title}", text);

            await _sessionManager.ClearSessionAsync(userId);

            var successMessage = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.NavigationAddSuccess, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(successMessage), ct);

            var menus = await _menuRepository.GetTopLevelMenusAsync();
            var template = await NavigationManageTemplate.CreateAsync(user.LanguageCode, _localizer, menus);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
