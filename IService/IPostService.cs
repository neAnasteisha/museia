using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using museia.Models;

public interface IPostService
{
    Task CreatePostAsync(string postText, string postPhoto, PostTag postTag, string userId);
    Task<Post> GetPostById(uint id);
    Task UpdatePost(Post post);
    Task DeletePost(uint id);
    Task<List<Post>> SearchPostsAsync(string searchText);
    List<SelectListItem> GetPostTags();
    Task<string> GetUserNicknameForPostAsync(int postId);
}
