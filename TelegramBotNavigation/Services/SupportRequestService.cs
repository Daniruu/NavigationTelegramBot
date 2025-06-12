using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramBotNavigation.Bot;
using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class SupportRequestService : ISupportRequestService
    {
        private readonly ISupportRequestRepository _requestRepository;
        private readonly ISupportMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<SupportRequestService> _logger;
        private readonly ITelegramClient _telegramClient;
        private readonly IBotSettingsService _settingsService;


        public SupportRequestService(
        ISupportRequestRepository requestRepository,
        ISupportMessageRepository messageRepository,
        IUserRepository userRepository,
        ILogger<SupportRequestService> logger,
        ITelegramClient telegramClient,
        IBotSettingsService botSettingsService)
        {
            _requestRepository = requestRepository;
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _logger = logger;
            _telegramClient = telegramClient;
            _settingsService = botSettingsService;
        }

        public async Task<SupportRequest> CreateRequestAsync(long userId, string message)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException($"User {userId} not found");

            var request = new SupportRequest
            {
                UserId = userId,
                User = user,
                Status = SupportStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var firstMessage = new SupportMessage
            {
                IsFromAdmin = false,
                Text = message,
                SentAt = DateTime.UtcNow,
                SupportRequest = request
            };

            request.Messages.Add(firstMessage);

            await _requestRepository.AddAsync(request);
            await _requestRepository.SaveChangesAsync();

            _logger.LogInformation("Support request {RequestId} created by user {UserId}", request.Id, userId);

            return request;
        }

        public async Task AddMessageAsync(int requestId, bool isFromAdmin, string text)
        {
            var request = await _requestRepository.GetByIdAsync(requestId)
                          ?? throw new InvalidOperationException($"Support request {requestId} not found");

            if (request.Status == SupportStatus.Resolved)
                throw new InvalidOperationException("Cannot add message to closed support request");

            if (isFromAdmin && request.Status != SupportStatus.InProgress)
            {
                request.Status = SupportStatus.InProgress;
                await _requestRepository.SaveChangesAsync();

                if (request.TopicId.HasValue)
                {
                    try
                    {
                        var chatId = await _settingsService.GetSupportGroupIdAsync();
                        if (chatId.HasValue)
                        {
                            await _telegramClient.EditForumTopicIconAsync(chatId.Value, request.TopicId.Value, "💬");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to edit/close forum topic for request {RequestId}", request.Id);
                    }
                }
            }

            var message = new SupportMessage
            {
                SupportRequestId = requestId,
                IsFromAdmin = isFromAdmin,
                Text = text,
                SentAt = DateTime.UtcNow
            };

            await _messageRepository.AddAsync(message);
            await _messageRepository.SaveChangesAsync();

            _logger.LogInformation("New message added to request {RequestId} (from admin: {IsFromAdmin})", requestId, isFromAdmin);
        }

        public async Task CloseRequestAsync(int requestId)
        {
            var request = await _requestRepository.GetByIdAsync(requestId)
                          ?? throw new InvalidOperationException($"Support request {requestId} not found");

            if (request.Status == SupportStatus.Resolved) return;

            request.Status = SupportStatus.Resolved;
            request.ClosedAt = DateTime.UtcNow;

            await _requestRepository.SaveChangesAsync();

            _logger.LogInformation("Support request {RequestId} closed", requestId);
        }

        public async Task<SupportRequest?> GetByIdAsync(int requestId)
        {
            return await _requestRepository.GetByIdAsync(requestId);
        }

        public async Task<SupportRequest?> GetLastOpenRequestForUserAsync(long userId)
        {
            return await _requestRepository
                .GetOpenRequestsAsync()
                .ContinueWith(t => t.Result
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault());
        }

        public async Task SetTelegramTopicAsync(int requestId, int topicId)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new InvalidOperationException($"Support request {requestId} not found");

            request.TopicId = topicId;
            request.UpdatedAt = DateTime.UtcNow;

            await _requestRepository.SaveChangesAsync();
        }

        public async Task SetAdminMessageIdAsync(int requestId, int adminMessageId)
        {
            var request = await _requestRepository.GetByIdAsync(requestId);
            if (request == null)
                throw new InvalidOperationException($"Support request {requestId} not found");

            request.AdminMessageId = adminMessageId;
            request.UpdatedAt = DateTime.UtcNow;

            await _requestRepository.SaveChangesAsync();

            _logger.LogInformation("AdminMessageId {AdminMessageId} set for support request {RequestId}", adminMessageId, requestId);
        }

        public async Task<SupportRequest?> GetRequestByTopicIdAsync(int topicId)
        {
            return await _requestRepository.GetByTopicIdAsync(topicId);
        }
    }
}
