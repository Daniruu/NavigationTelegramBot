using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Models;

namespace TelegramBotNavigation.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TelegramUser> TelegramUsers { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<UserInteraction> UserInteractions { get; set; }
        public DbSet<SupportRequest> SupportRequests { get; set; }
        public DbSet<WelcomeMessage> WelcomeMessages { get; set; }
        public DbSet<Translation> Translations { get; set; }
        public DbSet<TranslationImage> TranslationImages { get; set; }
        public DbSet<LanguageSetting> LanguageSettings { get; set; }
        public DbSet<NavigationMessage> NavigationMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Enum conversions
            modelBuilder.Entity<TelegramUser>()
                .Property(u => u.LanguageCode)
                .HasConversion<string>();

            modelBuilder.Entity<TelegramUser>()
                .Property(u => u.Role)
                .HasConversion<string>();

            modelBuilder.Entity<UserInteraction>()
                .Property(a => a.ActionType)
                .HasConversion<string>();

            modelBuilder.Entity<UserInteraction>()
                .Property(a => a.LanguageCode)
                .HasConversion<string>();

            modelBuilder.Entity<SupportRequest>()
                .Property(s => s.Status)
                .HasConversion<string>();

            modelBuilder.Entity<MenuItem>()
                .Property(m => m.ActionType)
                .HasConversion<string>();

            modelBuilder.Entity<WelcomeMessage>()
                .Property(w => w.LanguageCode)
                .HasConversion<string>();

            modelBuilder.Entity<LanguageSetting>()
                .Property(l => l.LanguageCode)
                .HasConversion<string>();

            modelBuilder.Entity<NavigationMessage>()
                .Property(n => n.LanguageCode)
                .HasConversion<string>();

            // Relationships
            modelBuilder.Entity<Menu>()
                .HasMany(m => m.MenuItems)
                .WithOne(m => m.Menu)
                .HasForeignKey(m => m.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SupportRequest>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.SubMenu)
                .WithMany()
                .HasForeignKey(mi => mi.SubMenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Menu>()
                .HasOne(m => m.ParentMenu)
                .WithMany()
                .HasForeignKey(m => m.ParentMenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserInteraction>()
                .HasOne(x => x.TelegramUser)
                .WithMany()
                .HasForeignKey(x => x.TelegramUserId);

        }
    }
}
