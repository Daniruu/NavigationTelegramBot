using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.DTOs;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class WelcomeMessageProvider : IWelcomeMessageProvider
    {
        private readonly IWelcomeMessageRepository _repository;
        private readonly ILanguageSettingRepository _langSettings;
        private readonly ILocalizationManager _localizer;

        public WelcomeMessageProvider(
            IWelcomeMessageRepository repository,
            ILanguageSettingRepository langSettings,
            ILocalizationManager localizer)
        {
            _repository = repository;
            _langSettings = langSettings;
            _localizer = localizer;
        }

        public async Task<WelcomeMessageDto> GetForUserAsync(LanguageCode userLang)
        {
            var fallbackOrder = await _langSettings.GetFallbackOrderAsync();
            fallbackOrder.Insert(0, userLang);
            fallbackOrder = fallbackOrder.Distinct().ToList();

            foreach (var lang in fallbackOrder)
            {
                var msg = await _repository.GetByLanguageCodeAsync(lang);
                if (msg != null)
                {
                    return new WelcomeMessageDto
                    {
                        Text = msg.Text,
                        ImageFileId = msg.ImageFileId
                    };
                }
            }

            var fallbackText = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.Welcome, userLang);
            return new WelcomeMessageDto { Text = fallbackText };
        }

        public async Task<WelcomeMessageResult> GetDetailedForAdminAsync(LanguageCode requestedLang)
        {
            var fallbackOrder = await _langSettings.GetFallbackOrderAsync();
            fallbackOrder.Insert(0, requestedLang);
            fallbackOrder = fallbackOrder.Distinct().ToList();

            foreach (var lang in fallbackOrder)
            {
                var msg = await _repository.GetByLanguageCodeAsync(lang);
                if (msg != null)
                {
                    bool isCustom = lang == requestedLang;
                    return new WelcomeMessageResult
                    {
                        Message = new WelcomeMessageDto
                        {
                            Text = msg.Text,
                            ImageFileId = msg.ImageFileId
                        },
                        IsCustom = isCustom
                    };
                }
            }

            var fallbackText = await _localizer.GetInterfaceTranslation(LocalizationKeys.Messages.Welcome, requestedLang);
            return new WelcomeMessageResult
            {
                Message = new WelcomeMessageDto { Text = fallbackText },
                IsCustom = false
            };
        }

    }
}
