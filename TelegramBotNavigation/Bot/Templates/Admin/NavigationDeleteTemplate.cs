using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;
using static TelegramBotNavigation.Bot.Shared.LocalizationKeys;
using static TelegramBotNavigation.Bot.Shared.CallbackKeys;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class NavigationDeleteTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode lang, ILocalizationManager localization, Menu menu)
        {
            var text = await localization.GetInterfaceTranslation(Messages.NavigationDeleteConfirmation, lang, menu.Title);

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localization.GetInterfaceTranslation(Labels.DeleteNavigationMenuConfirm, lang),
                        $"{NavigationDelete}:{menu.Id}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    await localization.GetInterfaceTranslation(Labels.CancelDeleteNavigation, lang),
                    $"{NavigationView}:{menu.Id}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    await localization.GetInterfaceTranslation(Labels.Back, lang),
                    $"{NavigationView}:{menu.Id}")
                }

            });

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
