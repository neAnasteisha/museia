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

        public async Task<bool> IsUserBlockedAsync(string userId)
        {
            return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.IsBlocked)
            .FirstOrDefaultAsync();
        }

        public async Task BlockUserAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.IsBlocked = true;
            }
            await _context.SaveChangesAsync();
        }
    }
}
