using TelegramBotNavigation.Enums;

namespace TelegramBotNavigation.DTOs
{
    public class PaginatedUserListDto
    {
        public List<UserSummaryDto> Users { get; set; } = [];
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }
}
