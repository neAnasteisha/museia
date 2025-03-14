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
            }
            await _context.SaveChangesAsync();
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

        public async Task<List<Post>> SearchPostsAsync(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return await _context.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync();
            }

            if (searchText.StartsWith("#"))
            {
                var tag = searchText.Substring(1);
                if (Enum.TryParse<PostTag>(tag, true, out var enumTag))
                {
                    return await _context.Posts
                        .Where(p => p.PostTag == enumTag)
                        .OrderByDescending(p => p.CreatedAt)
                        .ToListAsync();
                }
            }
            else
            {
                return await _context.Posts
                    .Where(p => EF.Functions.Like(p.PostText.ToLower(), $"%{searchText}%")) // Перетворюємо текст поста в нижній регістр
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            }

            return new List<Post>();
        }
    }
}
