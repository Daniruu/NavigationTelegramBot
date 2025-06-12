
using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Repositories;
using TelegramBotNavigation.Repositories.Interfaces;
using Telegram.Bot;
using TelegramBotNavigation.Bot;
using TelegramBotNavigation.Bot.CommandHandlers;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Threading.Tasks;
using TelegramBotNavigation.Bot.CallbackHandlers;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin;
using TelegramBotNavigation.Bot.CallbackHandlers.Common;
using TelegramBotNavigation.Bot.CommandHandlers.Admin;
using TelegramBotNavigation.Bot.CommandHandlers.Common;
using TelegramBotNavigation.Services.Sessions;
using TelegramBotNavigation.Bot.SessionHandlers;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin.Navigation;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin.WelcomeMessage;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin.Users;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin.Support;
using TelegramBotNavigation.Bot.CallbackHandlers.Admin.Settings;
using TelegramBotNavigation.Bot.MessageHandlers;

namespace TelegramBotNavigation
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration
            var botToken = builder.Configuration["TelegramBot:Token"];
            if (string.IsNullOrEmpty(botToken))
            {
                throw new InvalidOperationException("Telegram bot token is not configured.");
            }


            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");


            // Telegram Bot Client
            builder.Services.AddSingleton<ITelegramBotClient>(_ => new TelegramBotClient(botToken));

            // Entity Framework
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));

            // Bot
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<IUpdateProcessor, UpdateProcessor>();
            builder.Services.AddScoped<CommandDispatcher>();
            builder.Services.AddScoped<CallbackDispatcher>();
            builder.Services.AddScoped<SessionDispatcher>();
            builder.Services.AddScoped<MessageDispatcher>();
            builder.Services.AddSingleton<ITelegramClient, TelegramClient>();


            // Command Handlers
            builder.Services.AddScoped<ICommandHandler, StartCommandHandler>();
            builder.Services.AddScoped<ICommandHandler, LanguageCommandHandler>();
            builder.Services.AddScoped<ICommandHandler, AdminCommandHandler>();
            builder.Services.AddScoped<ICommandHandler, NavigationCommandHandler>();
            builder.Services.AddScoped<ICommandHandler, SetGroupCommandHandler>();
            builder.Services.AddScoped<ICommandHandler, SetAdminCommandHandler>();

            // Callback Handlers
            builder.Services.AddScoped<ICallbackHandler, AdminCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, LanguageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, WelcomeManageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, WelcomeEditCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, WelcomeRemoveImageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationManageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationAddCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationViewCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationRequestDeleteCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationDeleteCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationEditCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemAddCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SelectItemTypeCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemDeleteOptionsCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemDeleteCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ReorderCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ReorderSelectItemCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ReorderItemCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ReorderSaveCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemEditCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemEditLabelCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemEditUrlCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationHeaderEditCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, DeleteHeaderConfirmtaionCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, DeleteHeaderCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationHeaderImageEditCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, DeleteHeaderImageConfirmationCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, DeleteHeaderImageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, LanguageSettingsCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, LanguageMoveCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationSetDefaultCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ShowMessageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemEditMessageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, ItemRequestDeleteCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, NavigationMessageCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, UsersCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, UserDetailsCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SupportRequestCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SupportReplyCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SettingsCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SupportResolveCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, SetAdminCallbackHandler>();
            builder.Services.AddScoped<ICallbackHandler, UnsetAdminCallbackHandler>();

            // Session Handlers
            builder.Services.AddScoped<ISessionHandler, WelcomeEditSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, NavigationAddSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemAddLabelSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemAddUrlSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemEditLabelSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemEditUrlSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, NavigationHeaderEditSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, NavigationHeaderEditImageSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemAddMessageSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemEditMessageSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, ItemAddSubmenuSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, SupportRequestSessionHandler>();
            builder.Services.AddScoped<ISessionHandler, SupportReplySessionHandler>();

            //Message Handlers
            builder.Services.AddScoped<IMessageHandler, AdminReplyMessageHandler>();

            // Services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<ITranslationService, TranslationService>();
            builder.Services.AddScoped<ILocalizationManager, LocalizationManager>();
            builder.Services.AddScoped<ITelegramMessageService, TelegramMessageService>();
            builder.Services.AddScoped<ICommandSetupService, CommandSetupService>();
            builder.Services.AddScoped<IWelcomeMessageProvider, WelcomeMessageProvider>();
            builder.Services.AddScoped<IMenuButtonBuilder, MenuButtonBuilder>();
            builder.Services.AddScoped<ITranslationImageService, TranslationImageService>();
            builder.Services.AddScoped<INavigationMessageService, NavigationMessageService>();
            builder.Services.AddScoped<IUserInteractionService, UserInteractionService>();
            builder.Services.AddScoped<ICallbackAlertService, CallbackAlertService>();
            builder.Services.AddScoped<ISupportRequestService, SupportRequestService>();
            builder.Services.AddScoped<IBotSettingsService, BotSettingsService>();

            // Services for Sessions
            builder.Services.AddSingleton<ISessionManager, MemorySessionManager>();
            builder.Services.AddSingleton<IReorderSessionManager, MemoryReorderSessionManager>();

            // Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMenuRepository, MenuRepository>();
            builder.Services.AddScoped<IWelcomeMessageRepository, WelcomeMessageRepository>();
            builder.Services.AddScoped<ITranslationRepository, TranslationRepository>();
            builder.Services.AddScoped<ILanguageSettingRepository, LanguageSettingRepository>();
            builder.Services.AddScoped<ITranslationImageRepository, TranslationImageRepository>();
            builder.Services.AddScoped<INavigationMessageRepository, NavigationMessageRepository>();
            builder.Services.AddScoped<IUserInteractionRepository, UserInteractionRepository>();
            builder.Services.AddScoped<IMenuItemRepository, MenuItemRepository>();
            builder.Services.AddScoped<ISupportRequestRepository, SupportRequestRepository>();
            builder.Services.AddScoped<ISupportMessageRepository, SupportMessageRepository>();
            builder.Services.AddScoped<IBotSettingsRepository, BotSettingsRepository>();

            var app = builder.Build();

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en"),
                SupportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ru"), new CultureInfo("pl"), new CultureInfo("tr") },
                SupportedUICultures = new[] { new CultureInfo("en"), new CultureInfo("ru"), new CultureInfo("pl"), new CultureInfo("tr") }
            });

            // Swagger for development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();

            // Webhook endpoint
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await LanguageSettingsSeeder.SeedAsync(context);
            }

            app.Run();
        }
    }
}
