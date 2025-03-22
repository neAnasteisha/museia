using Microsoft.EntityFrameworkCore;
using museia.Models;
using museia.Repositories;

namespace museia.Services
{
    public class UserService : IUserService
    {
        private UserRepository _userRepository;
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllUsersAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateUserAsync(user);
        }
        public async Task DeleteUserAsync(string userId)
        {
            await _userRepository.DeleteUserAsync(userId);
        }

        public async Task<ProfileViewModel> GetProfileViewModelByIdAsync(string userId)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            var posts = await _userRepository.GetPostsByUserIdAsync(user.Id);

            return new ProfileViewModel
            {
                User = user,
                UserPosts = posts
            };
        }
    }
}
