using System.Reflection.Metadata;

namespace TelegramBotNavigation.Bot.Shared
{
    public static class LocalizationKeys
    {
        public static class Commands
        {
            public const string Start = "commands.start";
            public const string Language = "commands.language";
            public const string Navigation = "commands.navigation";
            public const string Admin = "commands.admin";
        }

        public static class Labels
        {
            public const string Confirm = "labels.confirm";
            public const string Edit = "labels.edit";
            public const string Save = "labels.save";
            public const string Cancel = "labels.cancel";
            public const string Delete = "labels.delete";
            public const string Back = "labels.back";

            public const string AdminPanel = "labels.admin.panel";
            public const string NavigationManage = "labels.navigation.manage";
            public const string WelcomeManage = "labels.welcome.manage";
            public const string BotSettings = "labels.bot.settings";
            public const string UsersManage = "labels.users.manage";
            public const string SupportRequests = "labels.support.requests";

            public const string DeleteImage = "labels.delete_image";
            public const string AddNavigationMenu = "labels.add.navigation.menu";
            public const string DeleteNavigationMenu = "labels.delete.navigation.menu";
            public const string AddNavigationItem = "labels.add.navigation.item";
            public const string MenuItemTypeUrl = "labels.menu_item_type.url";
            public const string MenuItemTypeSubMenu = "labels.menu_item_type.sub_menu";
            public const string MenuItemTypeShowMessage = "labels.menu_item_type.show_message";
            public const string MenuItemTypeSupportRequest = "labels.menu_item_type.support_request";
            public const string DeleteNavigationItem = "labels.delete.navigation.item";
            public const string ReorderNavigation = "labels.reorder.navigation";
            public const string EditNavigationItem = "labels.edit.navigation.item";
            public const string EditNavigationItemLabel = "labels.edit.navigation.item.label";
            public const string EditNavigationItemUrl = "labels.edit.navigation.item.url";
            public const string EditNavigationItemMessage = "labels.edit.navigation.item.message";
            public const string NavigationSetDefault = "lablers.navigation.set_default";
            public const string EditNavigation = "labels.navigation.edit";
            public const string EditMode = "labels.mode.edit";
            public const string DeleteMode = "labels.mode.delete";
            public const string NavigationHeaderEdit = "labels.nav.header_edit";
            public const string CancelDeleteNavigation = "labels.cancel.delete.navigation";
            public const string Language = "labels.language";
            public const string LastActivity = "labels.last_activity";
            public const string Status = "labels.status";
            public const string Blocked = "labels.blocked";
            public const string Unblocked = "labels.unblocked";
            public const string ViewProfile = "labels.view_profile";
            public const string SortAsc = "labels.sort.asc";
            public const string SortDesc = "labels.sort.desc";
        }

        public static class Headers
        {
            public const string AdminPanel = "headers.admin.panel";
            public const string WelcomeManage = "headers.welcome.manage";
            public const string LanguageSelection = "headers.language.selection";
            public const string NavigationManage = "headers.navigation.manage";
            public const string NavigationManageEmpty = "headers.navigation.manage.empty";
            public const string NavigationHeader = "headers.navigation";
            public const string NavigationView = "headers.navigation.view";
            public const string NavigationEdit = "headers.navigation.edit";
            public const string ItemTypeSelection = "headers.item_type_selection";
            public const string NavigationDeleteItem = "headers.item_delete";
            public const string NavigationReorder = "headers.navigation_reorder";
            public const string NavigationUrlItemEdit = "headers.navigation_url_item_edit";
            public const string NavigationMessageItemEdit = "headers.navigation_message_item_edit";
            public const string NavigationSubmenuItemEdit = "headers.navigation_submenu_item_edit";
            public const string UsersManage = "headers.users.manage";
            public const string UserDetails = "headers.user_details";
            public const string InteractionHistory = "headers.interaction_history";
        }

        public static class Messages
        {
            public const string Welcome = "messages.welcome";
            public const string WelcomeEditPrompt = "messages.welcome.edit.prompt";
            public const string LanguagePrompt = "messages.language.prompt";
            public const string SaveSuccess = "messages.save.success";
            public const string NavigationAddPrompt = "messages.navigation.add.prompt";
            public const string NavigationAddSuccess = "messages.navigation.add.success";
            public const string NavigationHasNoItems = "messages.navigation.no_items";
            public const string NavigationDeleteConfirmation = "messages.navigation.delete.confirmation";
            public const string EnterButtonLabel = "messages.enter.button_text";
            public const string EnterButtonValue = "messages.enter.button_value";
            public const string EnterButtonUrl = "messages.enter.button_url";
            public const string EnterButtonMessage = "messages.enter.button_message";
            public const string NotImplementedYet = "messages.not_implemented_yet";
            public const string NavigationHeaderEditPrompt = "messaegs.nav_header.edit.prompt";
            public const string NavigatiionItemDeleteConfirmation = "messages.navigation.item.delete.confirmation";
            public const string EnterMenuTitle = "messages.enter.menu.title";
            public const string UserExecuteCommand = "messages.user.execute_command";
            public const string UserClickButton = "messages.user.click_button";
            public const string UserClickSubmenu = "messages.user.click_submenu";
            public const string UserClickShowMessage = "messages.user.click_show_message";
            public const string UserChangedLanguage = "messages.user.change_language";
            public const string UserSentMessage = "messages.user.sent_message";
            public const string UserRequestedSupport = "messages.user.requested_support";
            public const string UserInteractionUnknown = "messages.user.interaction_unknown";
        }

        public static class Errors
        {
            public const string NotAdmin = "errors.not_admin";
            public const string UserNotFound = "errors.user_not_found";
            public const string InvalidCommand = "errors.invalid_command";
            public const string InvalidInput = "errors.invalid_input";
            public const string LanguageNotSupported = "errors.language_not_supported";
            public const string NolanguagesAvailable = "errors.no_languages_available";
            public const string SessionDataMissing = "errors.session_data_missing";
            public const string InvalidMenuId = "errors.invalid_menu_id";
            public const string MenuNotFound = "errors.menu_not_found";
            public const string InvalidActionType = "errors.invalid_action_type";
            public const string InvalidSessionData = "errors.invalid_session_data";
            public const string InvalidMenuItemId = "errors.invalid_menu_item_id";
            public const string InvalidUrl = "errors.invalid_url";
            public const string InvalidText = "errors.invalid_text";
            public const string ItemNotFound = "errors.item_not_found"; 
        }

        public static class Notifications
        {

            public const string LanguageChanged = "notifications.language.changed";
            public const string MenuItemAddSuccess = "notifications.menu_item.add.success";
            public const string MenuItemDeleteSuccess = "notifications.menu_item.delete.success";
            public const string MenuItemEditSuccess = "notifications.menu_item.edit.success";
            public const string NavigationHeaderEditSuccess = "notifications.nav_header.edit.success";
            public const string NavigationSetDefaultSuccess = "notifications.navigation.set_default.success";
            public const string WelcomeEditSuccess = "notifications.welcome.edit.success";
            public const string WelcomeImageRemoved = "notifications.welcome.image.removed";
            public const string NavigationDeleteSuccess = "notifications.navigation.delete.success";
        }
    }
}
