using System.Threading.Tasks;
using museia.Models;

public interface IUserService
{
    Task<User> GetUserByIdAsync(string userId);
    Task<List<User>> GetAllUsersAsync();
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
    Task<ProfileViewModel> GetProfileViewModelByIdAsync(string userId);
}