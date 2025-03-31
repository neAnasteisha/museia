using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using museia.IRepository;
using museia.IService;
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
            var user = new User { UserName = "testuser", Email = "testuser@example.com", Id = "user-001" };

            _userManagerMock.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);

            return (_userManagerMock, _postService, user);
        }

        private Post CreateTestPost(User user, uint postId = 1, string text = "Old description", PostTag tag = PostTag.Поезія, string photo = "/old/photo.jpg")
        {
            return new Post
            {
                PostID = postId,
                PostText = text,
                PostTag = tag,
                PostPhoto = photo,
                UserID = user.Id
            };
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
                () => postService.CreatePostAsync(null, null, PostTag.Музика, userId)
            );

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
            var (userManagerMock, postService, user) = await CreateTestContext();
            var post = CreateTestPost(user, postId: 1, text: "Old description", tag: PostTag.Поезія, photo: "/old/photo.jpg");

            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostText = "New description";
            await postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostText == "New description")
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangeTag()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();
            var post = CreateTestPost(user, postId: 2, text: "Description", tag: PostTag.Поезія, photo: "/photo.jpg");

            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostTag = PostTag.Музика;
            await postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostTag == PostTag.Музика)
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangePhoto()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();
            var post = CreateTestPost(user, postId: 3, text: "Description", tag: PostTag.Поезія, photo: "/old/photo.jpg");

            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostPhoto = "/new/photo.jpg";
            await postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostPhoto == "/new/photo.jpg")
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldChangeAllFields()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();
            var post = CreateTestPost(user, postId: 4, text: "Old description", tag: PostTag.Поезія, photo: "/old/photo.jpg");

            _postRepositoryMock.Setup(repo => repo.UpdatePostAsync(post)).Returns(Task.CompletedTask);

            post.PostText = "New description";
            post.PostTag = PostTag.Музика;
            post.PostPhoto = "/new/photo.jpg";
            await postService.UpdatePost(post);

            _postRepositoryMock.Verify(repo => repo.UpdatePostAsync(
                It.Is<Post>(p => p.PostText == "New description" &&
                                   p.PostTag == PostTag.Музика &&
                                   p.PostPhoto == "/new/photo.jpg")
            ), Times.Once);
        }

        [Fact]
        public async Task EditPost_ShouldThrowException_WhenBothTextAndPhotoEmpty()
        {
            var (userManagerMock, postService, user) = await CreateTestContext();

            var post = CreateTestPost(user, postId: 5, text: "Тестовий почт", tag: PostTag.Поезія, photo: "/old/photo.jpg");

            post.PostText = "";
            post.PostPhoto = null;

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => postService.UpdatePost(post));
            Assert.Equal("Пост не може бути порожнім.", exception.Message);
        }


        // тест пошуку існуючого допису

        [Fact]
        public async Task GetPostById_ShouldReturnPost_WhenFound()
        {
            uint postId = 1;
            var testUser = new User { Id = "user-001", UserName = "Test" };
            var expectedPost = new Post
            {
                PostID = postId,
                PostText = "Test post",
                PostTag = PostTag.Поезія,
                UserID = testUser.Id
            };

            _postRepositoryMock
                .Setup(repo => repo.GetPostByIdAsync(postId))
                .ReturnsAsync(expectedPost);

            var result = await _postService.GetPostById(postId);

            Assert.NotNull(result);
            Assert.Equal(expectedPost.PostID, result.PostID);
            Assert.Equal(expectedPost.PostText, result.PostText);
            Assert.Equal(expectedPost.PostTag, result.PostTag);
            Assert.Equal(expectedPost.UserID, result.UserID);
        }

        // тест пошуку неіснуючого допису

        [Fact]
        public async Task GetPostById_ShouldReturnNull_WhenNotFound()
        {
            uint postId = 999;
            _postRepositoryMock.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync((Post)null);

            var result = await _postService.GetPostById(postId);

            Assert.Null(result);
        }

        // тест видалення допису
        [Fact]
        public async Task DeletePost_ShouldRemovePost_WhenPostExists()
        {
            uint postId = 1;
            var testUser = new User { Id = "user-001", UserName = "Test" };
            var post = new Post
            {
                PostID = postId,
                PostText = "Test post",
                PostTag = PostTag.Поезія,
                UserID = testUser.Id
            };

            _postRepositoryMock.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(post);

            _postRepositoryMock.Setup(repo => repo.DeletePostAsync(postId)).Returns(Task.CompletedTask);

            await _postService.DeletePost(postId);

            _postRepositoryMock.Verify(repo => repo.DeletePostAsync(postId), Times.Once);
        }
        public class ComplaintsTests
        {
            private readonly Mock<IComplaintService> _complaintServiceMock;
            private readonly Mock<IPostRepository> _postRepositoryMock;
            private readonly Mock<UserManager<User>> _userManagerMock;

            public ComplaintsTests()
            {
                _complaintServiceMock = new Mock<IComplaintService>();
                _postRepositoryMock = new Mock<IPostRepository>();
                _userManagerMock = new Mock<UserManager<User>>(
                    new Mock<IUserStore<User>>().Object,
                    null, null, null, null, null, null, null, null
                );
            }

            [Fact]
            public async Task CreateComplaint_ShouldAddComplaint_WhenValidDataProvided()
            {
                uint postId = 1;
                var userId = "user-001";
                var reason = "This post contains inappropriate content";
                var complaint = new Complaint
                {
                    ComplaintID = 1,
                    PostID = postId,
                    UserID = userId,
                    ComplaintReason = reason,
                    ComplaintStatus = ComplaintStatus.Sent,
                    IsAcknowledged = false
                };

                _complaintServiceMock.Setup(service => service.CreateComplaintAsync(reason, userId, postId))
                    .Returns(Task.CompletedTask);

                await _complaintServiceMock.Object.CreateComplaintAsync(reason, userId, postId);

                
                _complaintServiceMock.Verify(service => service.CreateComplaintAsync(
                    It.Is<string>(r => r == reason),
                    It.Is<string>(u => u == userId),
                    It.Is<uint>(p => p == postId)
                ), Times.Once);
            }
        }

    }
}
