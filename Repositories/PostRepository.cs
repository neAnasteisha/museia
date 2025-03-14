namespace museia.Services
{
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;

    public class PostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
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

        public async Task AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }
    }
}
