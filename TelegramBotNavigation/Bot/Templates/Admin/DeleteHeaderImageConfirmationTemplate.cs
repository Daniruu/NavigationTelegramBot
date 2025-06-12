using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class DeleteHeaderImageConfirmationTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(int menuId, LanguageCode userLang, LanguageCode displayLang, ILocalizationManager localizer)
        {
            var text = await localizer.GetInterfaceTranslation(LocalizationKeys.Messages.DeleteHeaderImagePrompt, userLang, displayLang);

            var markup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.DeleteImage, userLang),
                        $"{CallbackKeys.DeleteHeaderImage}:{menuId}:{displayLang}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Cancel, userLang),
                        $"{CallbackKeys.NavigationEdit}:{menuId}:{displayLang}")
                }

            });

            return TelegramTemplate.Create(text, inline: markup);
        }
    }
}
