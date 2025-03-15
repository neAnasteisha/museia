namespace museia.Services
{
    using Microsoft.AspNetCore.Mvc.Rendering;
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

        public async Task<Post> GetPostByIdAsync(uint id)
        {
            return await _context.Posts.FindAsync(id);
        }

        public async Task UpdatePostAsync(Post post)
        {
            var existingPost = await _context.Posts.FindAsync(post.PostID);
            if (existingPost != null)
            {
                existingPost.PostText = post.PostText;
                existingPost.PostTag = post.PostTag;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePostAsync(uint id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Post>> GetPostsByTagAsync(PostTag tag)
        {
            return await _context.Posts
                .Where(p => p.PostTag == tag)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> SearchPostsByTextAsync(string searchText)
        {
            return await _context.Posts
                .Where(p => EF.Functions.Like(p.PostText.ToLower(), $"%{searchText.ToLower()}%"))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
