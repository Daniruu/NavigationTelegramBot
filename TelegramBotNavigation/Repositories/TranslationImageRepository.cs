using Microsoft.EntityFrameworkCore;
using TelegramBotNavigation.Data;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Repositories
{
    public class TranslationImageRepository : ITranslationImageRepository
    {
        private readonly ApplicationDbContext _context;

        public TranslationImageRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string?> GetFileIdAsync(string key, LanguageCode languageCode)
        {
            return await _context.TranslationImages.Where(t => t.Key == key && t.Language == languageCode.ToLanguageTag())
                .Select(ti => ti.FileId)
                .FirstOrDefaultAsync();
        }

        public async Task<TranslationImage?> GetAsync(string key, LanguageCode languageCode)
        {
            return await _context.TranslationImages.FirstOrDefaultAsync(t => t.Key == key && t.Language == languageCode.ToLanguageTag());
        }

        public async Task AddAsync(TranslationImage translationImage)
        {
            await _context.TranslationImages.AddAsync(translationImage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TranslationImage translationImage)
        {
            _context.TranslationImages.Update(translationImage);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string key, LanguageCode languageCode)
        {
            var translationImage = await GetAsync(key, languageCode);
            if (translationImage != null)
            {
                _context.TranslationImages.Remove(translationImage);
                await _context.SaveChangesAsync();
            }
        }
    }
}
