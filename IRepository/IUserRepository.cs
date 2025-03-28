using System.Collections.Generic;
using System.Threading.Tasks;
using museia.Models;

namespace museia.IRepository
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(string userId);
        Task<List<User>> GetAllUsersAsync();
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(string userId);

        Task<List<Post>> GetPostsByUserIdAsync(string userId);
    }
}