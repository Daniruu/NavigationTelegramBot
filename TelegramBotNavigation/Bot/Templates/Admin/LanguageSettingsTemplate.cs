using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class LanguageSettingsTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang, 
            ILocalizationManager localizer, 
            ILanguageSettingRepository 
            languageSettingRepository, 
            LanguageSetting? selectedLanguage = null)
        {
            var text = await localizer.GetInterfaceTranslation(Headers.LanguageSettings, userLang);

            var buttonList = new List<InlineKeyboardButton[]>();

            var languages = await languageSettingRepository.GetLanguageSettingsAsync();

            foreach (var lang in languages)
            {
                var isSelected = selectedLanguage == lang ? "»" : "";

                buttonList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{isSelected} {lang.LanguageCode.GetDisplayLabel()}", $"{LanguageSettings}:{lang.Id}")
                });
            }

            if (selectedLanguage != null)
            {
                buttonList.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("⬇️", $"{LanguageMove}:{selectedLanguage.Id}:Down"),
                    InlineKeyboardButton.WithCallbackData("⬆️", $"{LanguageMove}:{selectedLanguage.Id}:Up"),
                });
            }

            buttonList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(Labels.Back, userLang), AdminPanel)
            });

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, markup);
        }
    }
}
