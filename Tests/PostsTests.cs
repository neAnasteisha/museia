using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using museia.IRepository;
using museia.Models;
using museia.Services;
using Xunit;

namespace museia.Tests
{
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

            _postRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => p.PostID = 1);

            await postService.CreatePostAsync(postText, null, postTag, userId);

            _postRepositoryMock.Verify(repo => repo.AddAsync(
                It.Is<Post>(p => p.PostText == postText && p.PostTag == postTag && p.UserID == userId)
            ), Times.Once);
        }

        [Fact]
        public async Task CreatePost_ShouldAddPost_WhenPhotoIsProvided()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();

            var postPhoto = "/uploads/test.jpg";
            var postTag = PostTag.Малюнок;
            var userId = user.Id;

            _postRepositoryMock
                .Setup(repo => repo.AddAsync(It.IsAny<Post>()))
                .Callback<Post>(p => p.PostID = 1);

            await postService.CreatePostAsync(null, postPhoto, postTag, userId);

            _postRepositoryMock.Verify(repo => repo.AddAsync(
                It.Is<Post>(p => p.PostPhoto == postPhoto && p.PostTag == postTag && p.UserID == userId)
            ), Times.Once);
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

        [Fact]
        public async Task EditPost_ShouldChangeDescription()
        {
            var post = new Post
            {
                PostID = 1,
                PostText = "Old description",
                PostTag = PostTag.Поезія,
                PostPhoto = "/old/photo.jpg",
                UserID = "user-123"
            };
            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostText = "New description";
            await _postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostText == "New description")
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangeTag()
        {
            var post = new Post
            {
                PostID = 2,
                PostText = "Description",
                PostTag = PostTag.Поезія,
                PostPhoto = "/photo.jpg",
                UserID = "user-456"
            };
            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostTag = PostTag.Музика;
            await _postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostTag == PostTag.Музика)
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangePhoto()
        {
            var post = new Post
            {
                PostID = 3,
                PostText = "Description",
                PostTag = PostTag.Поезія,
                PostPhoto = "/old/photo.jpg",
                UserID = "user-789"
            };
            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostPhoto = "/new/photo.jpg";
            await _postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostPhoto == "/new/photo.jpg")
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangeAllFields()
        {
            var post = new Post
            {
                PostID = 4,
                PostText = "Old description",
                PostTag = PostTag.Поезія,
                PostPhoto = "/old/photo.jpg",
                UserID = "user-000"
            };
            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostText = "New description";
            post.PostTag = PostTag.Музика;
            post.PostPhoto = "/new/photo.jpg";
            await _postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostText == "New description" &&
                                   p.PostTag == PostTag.Музика &&
                                   p.PostPhoto == "/new/photo.jpg")
            ), Times.Once);
        }
    }
}
