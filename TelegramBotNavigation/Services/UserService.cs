using TelegramBotNavigation.Enums;
using TelegramBotNavigation.Models;
using TelegramBotNavigation.Repositories.Interfaces;
using TelegramBotNavigation.Services.Interfaces;

namespace TelegramBotNavigation.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<UserRole?> GetRoleAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return user.Role;
        }

        public async Task<bool> IsAdminAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            return user.Role == UserRole.Admin;
        }

        public bool IsAdmin(TelegramUser user)
        {
            if (user == null)
            {
                return false;
            }
            return user.Role == UserRole.Admin;
        }

        public async Task<bool> SetRoleAsync(long id, UserRole newRole)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            user.Role = newRole;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
