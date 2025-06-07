using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.CallbackHandlers.Common
{
    public class LanguageCallbackHandler : ICallbackHandler
    {
        public string Key => CallbackKeys.LanguageManage;

        private readonly IUserRepository _userRepository;
        private readonly ILocalizationManager _localizer;
        private readonly ICommandSetupService _commandSetupService;
        private readonly INavigationMessageService _navigationMessageService;
        private readonly IUserInteractionService _userInteractionService;
        private readonly ICallbackAlertService _callbackAlertService;

        public LanguageCallbackHandler(
            IUserRepository userRepository,
            ILocalizationManager localizer,
            ICommandSetupService commandSetupService,
            INavigationMessageService navigationMessageService,
            IUserInteractionService userInteractionService,
            ICallbackAlertService callbackAlertService)
        {
            _userRepository = userRepository;
            _localizer = localizer;
            _commandSetupService = commandSetupService;
            _navigationMessageService = navigationMessageService;
            _userInteractionService = userInteractionService;
            _callbackAlertService = callbackAlertService;
        }

        public async Task HandleAsync(CallbackQuery query, string[] args, CancellationToken ct)
        {
            if (args.Length == 0) return;

            var selectedLanguage = LanguageCodeHelper.FromTelegramTag(args[0]);

            var userId = query.From.Id;
            var chatId = query.Message!.Chat.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.LanguageCode = selectedLanguage;
            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _commandSetupService.SetupCommandsAsync(chatId, user, ct);

            await _userInteractionService.LogAsync(query.From, chatId, Enums.ActionType.LanguageChange, selectedLanguage.GetDisplayLabel());

            var alert = await _localizer.GetInterfaceTranslation(LocalizationKeys.Notifications.LanguageChanged, user.LanguageCode, selectedLanguage.GetDisplayLabel());
            await _callbackAlertService.ShowAsync(query.Id, alert, cancellationToken: ct);

            await _navigationMessageService.UpdateByChatIdAsync(chatId, user.LanguageCode, ct);
        }
    }
}
