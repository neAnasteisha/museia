namespace museia.Services
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;

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

        public async Task AddReactionAsync(Reaction reaction)
        {
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateReactionAsync(Reaction reaction)
        {
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostID == reaction.PostID && r.UserID == reaction.UserID);

            if (existingReaction != null)
            {
                existingReaction.ReactionType = reaction.ReactionType;
                _context.Reactions.Update(existingReaction);
            }
            else
            {
                _context.Reactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
        }

    }
}
