using Telegram.Bot.Types;
using TelegramBotNavigation.Bot.Templates.Common;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.CommandHandlers.Common
{
    public class LanguageCommandHandler : ICommandHandler
    {
        public string Command => "/language";
        
        private readonly IUserRepository _userRepository;
        private readonly ITelegramMessageService _messageService;
        private readonly ILocalizationManager _localizer;
        private readonly ILanguageSettingRepository _languageSettingRepository;
        private readonly IUserInteractionService _userInteractionService;

        public LanguageCommandHandler(
            IUserRepository userRepository,
            ITelegramMessageService messageService,
            ILocalizationManager localizer,
            ILanguageSettingRepository languageSettingRepository,
            IUserInteractionService userInteractionService)
        {
            _userRepository = userRepository;
            _messageService = messageService;
            _localizer = localizer;
            _languageSettingRepository = languageSettingRepository; ;
            _userInteractionService = userInteractionService;
        }

        public async Task HandleAsync(Message message, CancellationToken ct)
        {
            var userId = message.From!.Id;
            var chatId = message.Chat.Id;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return;

            user.LastActiveAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _userInteractionService.LogAsync(message.From, chatId, Enums.ActionType.Command, Command);

            var template = await LanguageSelectionTemplate.CreateAsync(user.LanguageCode, _languageSettingRepository, _localizer);
            await _messageService.SendTemplateAsync(chatId, template, ct);
        }
    }
}
