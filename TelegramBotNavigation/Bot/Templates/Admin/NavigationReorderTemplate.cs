using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services.Sessions;

namespace TelegramBotNavigation.Bot.Templates.Admin
{
    public static class NavigationReorderTemplate
    {
        public static async Task<TelegramTemplate> CreateAsync(
            LanguageCode userLang, 
            ILocalizationManager localizer,
            MenuReorderSession session)
        {

            var text = $"<b>{await localizer.GetInterfaceTranslation(LocalizationKeys.Headers.NavigationReorder, userLang)}</b>\n";

            var buttonRows = new List<InlineKeyboardButton[]>();

            foreach (var group in session.Items.GroupBy(item => item.Row).OrderBy(g => g.Key))
            {
                var row = new List<InlineKeyboardButton>();
                foreach (var item in group.OrderBy(i => i.Order))
                {
                    var label = item.Id == session.SelectedItemId
                       ? $"🟩 {item.Label}"
                       : item.Label;

                    var button = InlineKeyboardButton.WithCallbackData(label, $"{CallbackKeys.ReorderSelectItem}:{item.Id}");

                    row.Add(button);
                }
                buttonRows.Add(row.ToArray());
            }

            if (buttonRows.Count == 0)
            {
                text = $"{text}\n\n{await localizer.GetInterfaceTranslation(LocalizationKeys.Messages.NavigationHasNoItems, userLang)}";
            }

            var controlButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔼", $"{CallbackKeys.ReorderItem}:Up"),
                    InlineKeyboardButton.WithCallbackData("🔽", $"{CallbackKeys.ReorderItem}:Down"),
                    InlineKeyboardButton.WithCallbackData("◀️", $"{CallbackKeys.ReorderItem}:Left"),
                    InlineKeyboardButton.WithCallbackData("▶️", $"{CallbackKeys.ReorderItem}:Right")
                }
            };

            var manageButtons = new List<InlineKeyboardButton[]>
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Save, userLang), $"{CallbackKeys.ReorderSave}")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(await localizer.GetInterfaceTranslation(LocalizationKeys.Labels.Cancel, userLang), $"{CallbackKeys.NavigationEdit}:{session.MenuId}")
                }
            };

            var buttonList = new List<InlineKeyboardButton[]>();

            buttonList.AddRange(buttonRows);
            buttonList.AddRange(controlButtons);
            buttonList.AddRange(manageButtons);

            var markup = new InlineKeyboardMarkup(buttonList);

            return TelegramTemplate.Create(text, inline: markup, removeReplyKeyboard: true);
        }
    }
}
