using System.Threading.Tasks;
using museia.Models;

namespace museia.IService
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<List<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string userId);
        Task<ProfileViewModel> GetProfileViewModelByIdAsync(string userId);
        Task IsUserBlockedAsync(string userId);
        Task BlockUserAsync(string userId);
    }
}