namespace museia.Tests
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;
    using museia.Services;
    using Xunit;
    using museia.IRepository;
    using Moq;

    public class PostsTests
    {

        private readonly Mock<IPostRepository> _postRepositoryMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly PostService _postService;

        public PostsTests()
        {
            _postRepositoryMock = new Mock<IPostRepository>();
            _userManagerMock = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null
            );
            _postService = new PostService(_postRepositoryMock.Object);
        }

        private async Task<(Mock<UserManager<User>> userManagerMock, PostService postService, User user)> CreateTestContext()
        {
            var user = new User { UserName = "testuser", Email = "testuser@example.com" };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            return (_userManagerMock, _postService, user);
        }

        [Fact]
        public async Task CreatePost_ShouldAddPost_WhenTextIsProvided()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();

            var postText = "Це тестовий допис";
            var postTag = PostTag.Малюнок;
            var userId = user.Id;

            var post = new Post { PostText = postText, PostTag = postTag, UserID = userId };

            _postRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => p.PostID = 1);

            await postService.CreatePostAsync(postText, null, postTag, userId);

            _postRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Post>(p => p.PostText == postText && p.PostTag == postTag && p.UserID == userId)), Times.Once);
        }

        [Fact]
        public async Task CreatePost_ShouldAddPost_WhenPhotoIsProvided()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();

            var postPhoto = "/uploads/test.jpg";
            var postTag = PostTag.Малюнок;
            var userId = user.Id;

            var post = new Post { PostPhoto = postPhoto, PostTag = postTag, UserID = userId };

            _postRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => p.PostID = 1);

            await postService.CreatePostAsync(null, postPhoto, postTag, userId);

            _postRepositoryMock.Verify(repo => repo.AddAsync(It.Is<Post>(p => p.PostPhoto == postPhoto && p.PostTag == postTag && p.UserID == userId)), Times.Once);
        }

        [Fact]
        public async Task CreatePost_ShouldThrowException_WhenPostIsEmpty()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();

            var userId = user.Id;

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => postService.CreatePostAsync(null, null, PostTag.Музика, userId));

            Assert.Equal("Допис не може бути порожнім.", exception.Message);
        }

        [Fact]
        public async Task GetPostsByTag_ShouldReturnPostsByTag()
        {
            var searchText = "#Поезія";
            var expectedTag = PostTag.Поезія;
            var posts = new List<Post>
            {
                new Post { PostID = 1, PostTag = PostTag.Поезія, PostText = "Test Post 1", UserID = "user1" }
            };
            _postRepositoryMock
                .Setup(repo => repo.GetPostsByTagAsync(expectedTag))
                .ReturnsAsync(posts);

            var result = await _postService.SearchPostsAsync(searchText);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(expectedTag, result[0].PostTag);
        }
    }
}