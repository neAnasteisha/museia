using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using museia.Models;

namespace museia.IService
{
    public interface IPostService
    {
        Task CreatePostAsync(string postText, string postPhoto, PostTag postTag, string userId);
        Task<Post> GetPostById(uint id);
        Task UpdatePost(Post post);
        Task DeletePost(uint id);
        Task<List<Post>> SearchPostsAsync(string searchText);
        List<SelectListItem> GetPostTags();
        Task<string> GetUserNicknameForPostAsync(int postId);
        Task<List<Post>> GetPostsOfUserAsync(string userId);
        Task<string> GetUserIdByPostIdAsync(uint postId);
        Task MakePostHiddenAsync(uint id);
    }
}
