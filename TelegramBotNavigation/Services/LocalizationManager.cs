using Microsoft.Extensions.Localization;
using System.Globalization;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Services
{
    public class LocalizationManager : ILocalizationManager
    {
        private readonly ITranslationService _translationService;
        private readonly ITranslationImageService _translationImageService;
        private readonly IStringLocalizer _localizer;
        private readonly ILanguageSettingRepository _langSettings;
        private readonly ILogger<LocalizationManager> _logger;

        public LocalizationManager(
            ITranslationService translationService,
            ITranslationImageService imageTranslationService,
            IStringLocalizerFactory localizerFactory, 
            ILanguageSettingRepository langSettings, 
            ILogger<LocalizationManager> logger)
        {
            _translationService = translationService;
            _translationImageService = imageTranslationService;
            _localizer = localizerFactory.Create(typeof(SharedResources));
            _langSettings = langSettings;
            _logger = logger;
        }

        public async Task<string> GetInterfaceTranslation(string key, LanguageCode userLang, params object[] args)
        {
            var fallbackOrder = await _langSettings.GetFallbackOrderAsync();

            fallbackOrder.Insert(0, userLang);
            fallbackOrder = fallbackOrder.Distinct().ToList();

            foreach (var lang in fallbackOrder)
            {
                var fallbackCulture = lang.ToLanguageTag();
                CultureInfo.CurrentCulture = new CultureInfo(fallbackCulture);
                CultureInfo.CurrentUICulture = new CultureInfo(fallbackCulture);

                var localizedString = _localizer[key];

                if (!localizedString.ResourceNotFound && !string.IsNullOrWhiteSpace(localizedString.Value))
                {
                    return string.Format(localizedString.Value, args);
                }
            }

            return key;
        }

        public async Task<string?> GetCustomTranslationAsync(string key, LanguageCode userLang, params object[] args)
        {
            var fallbackOrder = await _langSettings.GetFallbackOrderAsync();

            fallbackOrder.Insert(0, userLang);
            fallbackOrder = fallbackOrder.Distinct().ToList();

            foreach (var lang in fallbackOrder)
            {
                var custom = await _translationService.GetTranslationAsync(key, lang);
                if (!string.IsNullOrEmpty(custom))
                {
                    return string.Format(custom, args);
                }
            }

            return null;
        }

        public async Task<string?> GetImageTranslationAsync(string key, LanguageCode userLang)
        {
            var imageFileId = await _translationImageService.GetTranslationImageAsync(key, userLang);
            if (!string.IsNullOrWhiteSpace(imageFileId))
            {
                return imageFileId;
            }

            return null;
        }

    }
}
