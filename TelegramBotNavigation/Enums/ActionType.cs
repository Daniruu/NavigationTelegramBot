namespace TelegramBotNavigation.Enums
{
    public enum ActionType
    {
        Command,            // Пользователь выполнил команду
        SubmenuClick,       // Пользователь нажал на кнопку подменю
        ShowMessageClick,   // Пользователь нажал на кнопку для показа сообщения
        Message,            // Пользователь отправил сообщение
        LanguageChange,     // Пользователь изменил язык
        SupportRequest      // Пользователь запросил поддержку
    }
}
