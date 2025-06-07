using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class WelcomeManageTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang, 
            LanguageCode messageLang, 
            ILocalizationManager localizer, 
            ILanguageSettingRepository languageSettingRepository,
            IWelcomeMessageProvider welcomeMessageProvider)
        {
            var langLabel = messageLang.GetDisplayLabel();

            var text = $"<b>{await localizer.GetInterfaceTranslation(LocalizationKeys.Headers.WelcomeManage, userLang)} ({langLabel})</b>";

            var result = await welcomeMessageProvider.GetDetailedForAdminAsync(messageLang);
            var welcomeMessage = result.Message;
            var isCustom = result.IsCustom;

            text = $"{text}\n\n{welcomeMessage.Text}";

            var welcomePhoto = welcomeMessage.ImageFileId;

            var languages = await languageSettingRepository.GetFallbackOrderAsync();

            var buttonList = new List<InlineKeyboardButton[]>();

            var languageButtons = languages.Select(lang =>
            {
                var flag = lang switch
                {
                    LanguageCode.Tr => "🇹🇷",
                    LanguageCode.En => "🇬🇧",
                    LanguageCode.Ru => "🇷🇺",
                    LanguageCode.Pl => "🇵🇱",
                    _ => lang.ToString()
                };

                var isSelected = lang == messageLang ? "»" : "";

                return InlineKeyboardButton.WithCallbackData($"{isSelected} {flag}", $"{CallbackKeys.WelcomeManage}:{lang.ToLanguageTag()}");
            })
            .ToArray();

            buttonList.Add(languageButtons);

            buttonList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Edit, userLang), $"{CallbackKeys.WelcomeEdit}:{messageLang.ToLanguageTag()}")
            });

            if (isCustom && !string.IsNullOrEmpty(welcomePhoto))
            {
                buttonList.Add(new[]
 {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.DeleteImage, userLang), $"{CallbackKeys.WelcomeRemoveImage}:{messageLang.ToLanguageTag()}")
                });
            }

            buttonList.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Back, userLang), CallbackKeys.AdminPanel)
            });

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true, photo: welcomePhoto);
        }
    }
}
