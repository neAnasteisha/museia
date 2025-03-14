namespace museia.Services
{
    using museia.Models;

    public class PostService
    {
        private PostRepository _postRepository;

        public PostService(PostRepository postRepository)
        {
            _postRepository = postRepository;
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
    }
}
