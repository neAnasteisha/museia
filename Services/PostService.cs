namespace museia.Services
{
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;

    public class PostService
    {
        private readonly AppDbContext _context;
        public PostService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Post>> GetAllPostsAsync()
        {
            var posts = await _context.Posts
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return posts;
        }
    }
}
