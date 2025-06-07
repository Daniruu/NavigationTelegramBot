using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;

namespace TelegramBotNavigation.Repositories
{
    public class WelcomeMessageRepository : IWelcomeMessageRepository
    {
        private readonly ApplicationDbContext _context;
        public WelcomeMessageRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<WelcomeMessage?> GetByLanguageCodeAsync(LanguageCode languageCode)
        {
            return await _context.WelcomeMessages
                .FirstOrDefaultAsync(wm => wm.LanguageCode == languageCode);
        }

        public async Task SetAsync(LanguageCode languageCode, string? text, string? imageFileId)
        {
            var welcomeMessage = await GetByLanguageCodeAsync(languageCode);
            if (welcomeMessage == null)
            {
                welcomeMessage = new WelcomeMessage
                {
                    LanguageCode = languageCode,
                    Text = text ?? string.Empty,
                    ImageFileId = imageFileId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _context.WelcomeMessages.AddAsync(welcomeMessage);
            }
            else
            {
                if (text != null) 
                    welcomeMessage.Text = text;

                if (imageFileId != null)
                    welcomeMessage.ImageFileId = imageFileId;

                welcomeMessage.UpdatedAt = DateTime.UtcNow;

                _context.WelcomeMessages.Update(welcomeMessage);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RemoveImageAsync(LanguageCode languageCode)
        {
            var welcomeMessage = await GetByLanguageCodeAsync(languageCode);
            if (welcomeMessage != null)
            {
                welcomeMessage.ImageFileId = null;
                welcomeMessage.UpdatedAt = DateTime.UtcNow;

                _context.WelcomeMessages.Update(welcomeMessage);
                await _context.SaveChangesAsync();
            }
        }
    }
}
