namespace museia.Services
{
    using Microsoft.AspNetCore.Mvc.Rendering;
    using museia.Models;

    public class PostService
    {
        private PostRepository _postRepository;

        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        public List<SelectListItem> GetPostTags()
        {
            return Enum.GetValues(typeof(PostTag))
                       .Cast<PostTag>()
                       .Select(t => new SelectListItem
                       {
                           Value = t.ToString(),
                           Text = t.ToString()
                       })
                       .ToList();
        }

        public async Task CreatePostAsync(string postText, string postPhoto, PostTag postTag, string userId)
        {
            //if (string.IsNullOrWhiteSpace(postText) && string.IsNullOrEmpty(postPhoto))
            //{
            //    throw new ArgumentException("Post can't be empty.");
            //}

            var post = new Post
            {
                PostText = postText,
                PostPhoto = postPhoto,
                PostTag = postTag,
                UserID = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.AddAsync(post);
        }

        public async Task<Post> GetPostById(uint id)
        {
            return await _postRepository.GetPostByIdAsync(id);
        }

        public async Task UpdatePost(Post post)
        {
            await _postRepository.UpdatePostAsync(post);
        }



        public async Task DeletePost(uint id)
        {
            await _postRepository.DeletePostAsync(id);
        }

        public async Task<List<Post>> SearchPostsAsync(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return await _postRepository.GetAllPostsAsync();
            }

            if (searchText.StartsWith("#"))
            {
                var tag = searchText.Substring(1);
                if (Enum.TryParse<PostTag>(tag, true, out var enumTag))
                {
                    return await _postRepository.GetPostsByTagAsync(enumTag);
                }
            }
            else
            {
                return await _postRepository.SearchPostsByTextAsync(searchText);
            }

            return new List<Post>();
        }

    }
}
