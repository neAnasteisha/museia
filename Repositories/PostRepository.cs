namespace museia.Services
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;
    using museia.IRepository;

    public class PostRepository : IPostRepository
    {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.Reactions)
                .ThenInclude(r => r.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }


        public async Task AddAsync(Post post)
        {
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
        }

        public async Task<Post> GetPostByIdAsync(uint id)
        {
            return await _context.Posts
            .Include(p => p.Reactions)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(p => p.PostID == id);
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
            var allPosts = await _context.Posts
                .Where(p => p.PostText != null)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var loweredSearch = searchText.ToLower();

            var filteredPosts = allPosts
                .Where(p => p.PostText.ToLower().Contains(loweredSearch))
                .ToList();

            return filteredPosts;
        }

        public async Task<List<Post>> GetPostsByUserId(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserID == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<string> GetUserNicknameForPostAsync(int postId)
        {
            var post = await _context.Posts
                .Include(p => p.User) 
                .FirstOrDefaultAsync(p => p.PostID == postId);

            if (post == null || post.User == null)
            {
                return "Невідомий користувач";
            }

            return post.User.UserName;
        }

        public async Task<string> GetUserIdByPostIdAsync(uint postId)
        {
            var post = await GetPostByIdAsync(postId);
            return post.UserID.ToString();
        }
    }

}
