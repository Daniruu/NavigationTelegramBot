using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public class ItemDeleteTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(LanguageCode lang, LanguageCode displayLang, ILocalizationManager localization, int menuId, int menuItemId)
        {
            var text = await localization.GetInterfaceTranslation(LocalizationKeys.Messages.NavigatiionItemDeleteConfirmation, lang);

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localization.GetInterfaceTranslation(LocalizationKeys.Labels.Delete, lang),
                        $"{CallbackKeys.ItemDelete}:{menuId}:{menuItemId}:{displayLang}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                    await localization.GetInterfaceTranslation(LocalizationKeys.Labels.Cancel, lang),
                    $"{CallbackKeys.ItemDeleteOptions}:{menuId}:{displayLang}")
                }

            });

            return TelegramTemplate.Create(text, inline: markup);
        }
    }
}
