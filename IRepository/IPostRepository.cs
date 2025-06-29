﻿using System.Collections.Generic;
using System.Threading.Tasks;
using museia.Models;

namespace museia.IRepository
{
    public interface IPostRepository
    {
        Task AddAsync(Post post);
        Task<Post> GetPostByIdAsync(uint id);
        Task UpdatePostAsync(Post post);
        Task DeletePostAsync(uint id);
        Task<List<Post>> GetAllPostsAsync();
        Task<List<Post>> GetPostsByTagAsync(PostTag postTag);
        Task<List<Post>> GetPostsByUsersNickAsync(string nick);
        Task<List<Post>> SearchPostsByTextAsync(string searchText);
        Task<string> GetUserNicknameForPostAsync(int postId);
        Task<string> GetUserIdByPostIdAsync(uint postId);
        Task<List<Post>> GetPostsByUserId(string userId);
        Task MakePostHiddenAsync(uint id);
    }
}
