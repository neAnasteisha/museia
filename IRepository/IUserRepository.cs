using System.Collections.Generic;
using System.Threading.Tasks;
using museia.Models;

public interface IUserRepository
{
    Task<User> GetUserByIdAsync(string userId);
    Task<List<User>> GetAllUsersAsync();
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);
    Task<User> RegisterUserAsync(string username, string email, string password);
    Task<User> AuthenticateUserAsync(string username, string password);
}
