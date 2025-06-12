using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Common
{
    public class SupportRequestCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.SupportRequest;

        private readonly ISessionManager _sessionManager;
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;

        public SupportRequestCallbackHandler(
            ISessionManager sessionManager,
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer)
        {
            _sessionManager = sessionManager;
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            var userId = query.From.Id;
            var chatId = query.Message!.Chat.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            await _sessionManager.ClearSessionAsync(userId);

            await _sessionManager.SetSessionAsync(userId, new SessionData { Action = SessionKeys.SupportRequest }, TimeSpan.FromMinutes(10));

            var prompt = await _localizer.GetInterfaceTranslation(Messages.EnterSupportRequestMessage, user.LanguageCode);
            await _messageService.SendTemplateAsync(chatId, TelegramTemplate.Create(prompt), ct);
        }
    }
}
