using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.Templates.Common
{
    public static class LanguageSelectionTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode userLang, ILanguageSettingRepository languageSettingRepository, ILocalizationManager localizer)
        {
            var text = await localizer.GetInterfaceTranslation(LocalizationKeys.Messages.LanguagePrompt, userLang);

            var languages = await languageSettingRepository.GetFallbackOrderAsync();

            if (languages.Count == 0)
            {
                return TelegramTemplate.Create(await localizer.GetInterfaceTranslation(LocalizationKeys.Errors.NolanguagesAvailable, userLang));
            }

            var markup = languages.Select(lang =>
            {
                var label = lang switch
                {
                    LanguageCode.Tr => "🇹🇷 Türkçe",
                    LanguageCode.En => "🇬🇧 English",
                    LanguageCode.Ru => "🇷🇺 Русский",
                    LanguageCode.Pl => "🇵🇱 Polski",
                    _ => lang.ToString()
                };

                return InlineKeyboardButton.WithCallbackData(label, $"{CallbackKeys.LanguageManage}:{lang.ToLanguageTag()}");
            })
            .Chunk(2)
            .Select(row => row.ToArray())
            .ToArray();

            return TelegramTemplate.Create(text, inline: new InlineKeyboardMarkup(markup), removeReplyKeyboard: true);
        }
    }
}
