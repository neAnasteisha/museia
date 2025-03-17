using System.Collections.Generic;
using System.Threading.Tasks;
using museia.Models;

public interface IPostRepository
{
    Task AddAsync(Post post);
    Task<Post> GetPostByIdAsync(uint id);
    Task UpdatePostAsync(Post post);
    Task DeletePostAsync(uint id);
    Task<List<Post>> GetAllPostsAsync();
    Task<List<Post>> GetPostsByTagAsync(PostTag postTag);
    Task<List<Post>> SearchPostsByTextAsync(string searchText);
    Task AddReactionAsync(Reaction reaction);
}
