using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Bot.Templates;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class AdminPanelMainTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode lang, ILocalizationManager localizer)
        {
            var text = await localizer.GetInterfaceTranslation(LocalizationKeys.Headers.AdminPanel, lang);

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.NavigationManage, lang), CallbackKeys.NavigationManage)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.WelcomeManage, lang), CallbackKeys.WelcomeManage)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.LanguageSettings, lang), CallbackKeys.LanguageSettings)
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.UsersManage, lang), CallbackKeys.UsersManage)
                }
            });

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
