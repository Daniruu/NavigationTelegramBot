using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class SettingsTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode lang, ILocalizationManager localizer)
        {
            var header = await localizer.GetInterfaceTranslation(LocalizationKeys.Headers.Settings, lang);

            return TelegramTemplate.Create(text: header);
        }
    }
}
