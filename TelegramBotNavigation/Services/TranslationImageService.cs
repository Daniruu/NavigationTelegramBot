using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;
using TelegramBotNavigation.Utils;

namespace TelegramBotNavigation.Services
{
    public class TranslationImageService : ITranslationImageService
    {
        private readonly ITranslationImageRepository _translationImageRepository;

        public TranslationImageService(ITranslationImageRepository translationImageRepository)
        {
            _translationImageRepository = translationImageRepository;
        }

        public async Task<string?> GetTranslationImageAsync(string key, LanguageCode languageCode)
        {
            return await _translationImageRepository.GetFileIdAsync(key, languageCode);
        }

        public async Task SetTranslationImageAsync(string key, LanguageCode languageCode, string fileId)
        {
            var translationImage = await _translationImageRepository.GetAsync(key, languageCode);

            if (translationImage != null)
            {
                translationImage.FileId = fileId;
                await _translationImageRepository.UpdateAsync(translationImage);
            }
            else
            {
                var newTranslationImage = new TranslationImage
                {
                    Key = key,
                    Language = languageCode.ToLanguageTag(),
                    FileId = fileId
                };
                await _translationImageRepository.AddAsync(newTranslationImage);
            }
        }

        public async Task DeleteTranslationImageAsync(string key, LanguageCode languageCode)
        {
            await _translationImageRepository.DeleteAsync(key, languageCode);
        }
    }
}
