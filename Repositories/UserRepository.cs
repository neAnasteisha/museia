namespace museia.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;
    using System.ComponentModel;
    using System.Reflection.Metadata.Ecma335;
    using museia.IRepository;

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User not found.");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Post>> GetPostsByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserID == userId)
                .Include(p => p.Reactions)  
                .ToListAsync();
        }

        public async Task IncrementWarningCounter(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                var property = typeof(User).GetProperty("CountOfWarnings");
                if (property != null && property.PropertyType == typeof(int))
                {
                    int currentValue = (int)property.GetValue(user);
                    property.SetValue(user, currentValue + 1);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<int> GetUserWarningCounterByUserId(string userId)
        {
            var user = _context.Users.FindAsync(userId);
            if (user != null)
            {
                var property = typeof(User).GetProperty("");
                if (property != null && property.PropertyType == typeof(int))
                {
                    return (int)property.GetValue(user);
                }
            }
            return 0;
        }
    }
}
