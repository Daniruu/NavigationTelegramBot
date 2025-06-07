using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.CallbackHandlers.Common;
using TelegramBotNavigation.Bot.Templates.Start;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Services
{
    public class NavigationMessageService : INavigationMessageService
    {
        private readonly ITelegramBotClient _bot;
        private readonly INavigationMessageRepository _repository;
        private readonly ILocalizationManager _localizer;
        private readonly IMenuRepository _menuRepository;
        private readonly IMenuButtonBuilder _menuButtonBuilder;
        private readonly ITelegramMessageService _messageService;
        private readonly ILogger<NavigationMessageService> _logger;

        public NavigationMessageService(
            ITelegramBotClient bot,
            INavigationMessageRepository repository,
            ILocalizationManager localizer,
            IMenuRepository menuRepository,
            IMenuButtonBuilder menuButtonBuilder,
            ITelegramMessageService messageService,
            ILogger<NavigationMessageService> logger)
        {
            _bot = bot;
            _repository = repository;
            _localizer = localizer;
            _menuRepository = menuRepository;
            _menuButtonBuilder = menuButtonBuilder;
            _messageService = messageService;
            _logger = logger;
        }

        public async Task SendNavigationAsync(long chatId, LanguageCode languageCode, CancellationToken ct)
        {
            var menu = await _menuRepository.GetMainMenuAsync();
            if (menu == null) return;

            var exitingMessages = await _repository.GetByChatIdAsync(chatId);
            foreach (var msg in exitingMessages)
            {
                try
                {
                    await _bot.DeleteMessage(msg.ChatId, msg.MessageId, ct);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    if (!ex.Message.Contains("message to delete not found"))
                    {
                        _logger.LogWarning(ex, "Failed to delete navigation message ChatId {ChatId}, MsgId {MessageId}", msg.ChatId, msg.MessageId);
                    }
                }

                await _repository.DeleteByIdAsync(msg.Id);
            }

            var template = await NavigationTemplate.CreateAsync(languageCode, _localizer, _menuButtonBuilder, menu);
            var sentMessage = await _messageService.SendTemplateAsync(chatId, template, ct);

            var navMessage = new NavigationMessage
            {
                ChatId = chatId,
                MessageId = sentMessage.MessageId,
                LanguageCode = languageCode,
                LastUpdated = DateTime.UtcNow
            };

            await _repository.AddAsync(navMessage);
        }

        public async Task UpdateAllNavigationMessagesAsync(CancellationToken ct)
        {
            var messages = await _repository.GetAllAsync();
            var menu = await _menuRepository.GetMainMenuAsync();
            if (menu == null) return;

            foreach (var msg in messages)
            {
                try
                {
                    var template = await NavigationTemplate.CreateAsync(
                        msg.LanguageCode,
                        _localizer,
                        _menuButtonBuilder,
                        menu);

                    await _messageService.EditTemplateAsync(msg.ChatId, msg.MessageId, template, ct);

                    msg.LastUpdated = DateTime.UtcNow;
                    await _repository.UpdateAsync(msg);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    if (ex.Message.Contains("message to edit not found"))
                    {
                        await _repository.DeleteByIdAsync(msg.Id);
                    }
                    else
                    {
                        _logger.LogError(ex, "Failed to update navigation message for ChatId {ChatId}, MessageId {MessageId}", msg.ChatId, msg.MessageId);
                    }
                }

            }
        }

        public async Task UpdateByChatIdAsync(long chatId, LanguageCode languageCode, CancellationToken ct)
        {
            var messages = await _repository.GetByChatIdAsync(chatId);

            var menu = await _menuRepository.GetMainMenuAsync();
            if (menu == null) return;

            foreach (var msg in messages)
            {
                try
                {
                    var template = await NavigationTemplate.CreateAsync(
                        languageCode,
                        _localizer,
                        _menuButtonBuilder,
                        menu);

                    await _messageService.EditTemplateAsync(msg.ChatId, msg.MessageId, template, ct);

                    msg.LastUpdated = DateTime.UtcNow;
                    msg.LanguageCode = languageCode;
                    await _repository.UpdateAsync(msg);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    if (ex.Message.Contains("message to edit not found"))
                    {
                        await _repository.DeleteByIdAsync(msg.Id);
                    }
                    else
                    {
                        _logger.LogError(ex, "Failed to update navigation message for ChatId {ChatId}, MessageId {MessageId}", msg.ChatId, msg.MessageId);
                    }
                }
            }
        }
    }
}
