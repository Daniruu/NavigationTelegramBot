using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotNavigation.Bot.Shared;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class MenuButtonBuilder : IMenuButtonBuilder
    {
        private readonly ILocalizationManager _localizer;

        public MenuButtonBuilder(ILocalizationManager localizer)
        {
            _localizer = localizer;
        }

        public async Task<InlineKeyboardButton> BuildButtonAsync(MenuItem item, LanguageCode lang, MenuButtonContext context)
        {
            var label = await _localizer.GetCustomTranslationAsync(item.LabelTranslationKey, lang) ?? "[No label]";

            switch (item.ActionType)
            {
                
                case MenuActionType.Url:
                    return InlineKeyboardButton.WithUrl(label, item.Url!);

                case MenuActionType.ShowMessage:
                    return InlineKeyboardButton.WithCallbackData(label, $"{CallbackKeys.ShowMessage}:{item.Id}:{lang}");

                case MenuActionType.SubMenu:
                    string callback = context switch
                    {
                        MenuButtonContext.User => $"{CallbackKeys.NavigationMessage}:{item.SubMenuId}:{item.Id}",
                        MenuButtonContext.AdminView => $"{CallbackKeys.NavigationView}:{item.SubMenuId}:{lang}",
                        _ => "noop"
                    };
                    return InlineKeyboardButton.WithCallbackData(label, callback);

                default:
                    return InlineKeyboardButton.WithCallbackData(label, "noop");
            }
        }
    }
}
