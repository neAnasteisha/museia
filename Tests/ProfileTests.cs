namespace museia.Tests
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using museia.Controllers;
    using museia.IRepository;
    using museia.IService;
    using museia.Models;
    using museia.Services;
    using Xunit;

    public class ProfileTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly UserService _userService;

        public ProfileTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object,
                null, null, null, null, null, null, null, null);

            _mockUserRepository = new Mock<IUserRepository>();
            _userService = new UserService(_mockUserRepository.Object);
        }

        [Fact]
        public async Task DeleteProfile_ShouldRemoveUser()
        {
            var user = new User { UserName = "testuser", Email = "testuser@example.com", Id = Guid.NewGuid().ToString() };

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserManager.Setup(um => um.DeleteAsync(It.IsAny<User>()))
                            .ReturnsAsync(IdentityResult.Success);

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(It.IsAny<string>()))
                               .ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.DeleteUserAsync(It.IsAny<string>()))
                               .Returns(Task.CompletedTask);

            await _userService.DeleteUserAsync(user.Id);

            _mockUserRepository.Verify(repo => repo.DeleteUserAsync(user.Id), Times.Once);
        }

        [Fact]
        public async Task OtherUserProfile_ShouldReturnNotFound_WhenIdIsNullOrEmpty()
        {
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            var mockComplaintService = new Mock<IComplaintService>();
            var mockPostService = new Mock<IPostService>();
            var mockReactionService = new Mock<IReactionService>();
            var mockUserService = new Mock<IUserService>();

            var controller = new UserController(null, mockUserManager.Object, mockComplaintService.Object,
                                                  mockPostService.Object, mockReactionService.Object, mockUserService.Object);

            var result = await controller.OtherUserProfile(null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task OtherUserProfile_ShouldReturnViewWithProfileViewModel_WhenProfileExists()
        {
            var mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null);
            var mockComplaintService = new Mock<IComplaintService>();
            var mockPostService = new Mock<IPostService>();
            var mockReactionService = new Mock<IReactionService>();
            var mockUserService = new Mock<IUserService>();

            var expectedProfile = new ProfileViewModel
            {
                User = new User { Id = "user-123", UserName = "TestUser" },
                UserPosts = new List<Post>()
            };

            mockUserService.Setup(us => us.GetProfileViewModelByIdAsync("user-123")).ReturnsAsync(expectedProfile);

            var controller = new UserController(null, mockUserManager.Object, mockComplaintService.Object,
                                                  mockPostService.Object, mockReactionService.Object, mockUserService.Object);

            var result = await controller.OtherUserProfile("user-123");

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProfileViewModel>(viewResult.Model);
            Assert.Equal(expectedProfile, model);
        }

        [Fact]
        public async Task EditProfile_ShouldUpdateUser()
        {
            var userId = "123";
            var user = new User { Id = userId, UserName = "OldName", UserDescription = "OldDescription", UserAvatar = "old.png" };
            var model = new EditProfileViewModel { Username = "NewName", Description = "NewDescription", AvatarUrl = "new.png" };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId))
                               .ReturnsAsync(user);

            _mockUserRepository.Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
                               .Returns(Task.CompletedTask);

            user.UserName = model.Username;
            user.UserDescription = model.Description;
            user.UserAvatar = model.AvatarUrl;

            await _userService.UpdateUserAsync(user);

            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(It.Is<User>(u =>
                u.UserName == model.Username &&
                u.UserDescription == model.Description &&
                u.UserAvatar == model.AvatarUrl
            )), Times.Once);
        }

        [Fact]
        public async Task GetProfileViewModelByIdAsync_ShouldReturnOwnProfile_WhenUserExists()
        {
            var userId = "i-am-user-123";
            var user = new User { Id = userId, UserName = "IAmUser", Email = "myuser@example.com" };
            var userPosts = new List<Post>
            {
                new Post { PostID = 1, PostText = "My first post", UserID = userId, CreatedAt = DateTime.UtcNow }
            };

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(user);
            _mockUserRepository.Setup(repo => repo.GetPostsByUserIdAsync(userId)).ReturnsAsync(userPosts);

            var result = await _userService.GetProfileViewModelByIdAsync(userId);

            Assert.NotNull(result);
            Assert.Equal(user, result.User);
            Assert.Equal("IAmUser", result.User.UserName);
            Assert.Single(result.UserPosts);
            Assert.Equal("My first post", result.UserPosts[0].PostText);
            _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once());
            _mockUserRepository.Verify(repo => repo.GetPostsByUserIdAsync(userId), Times.Once());
        }

        [Fact]
        public async Task GetProfileViewModelByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            var userId = "non-existent-user";

            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync((User)null);

            var result = await _userService.GetProfileViewModelByIdAsync(userId);

            Assert.Null(result);
            _mockUserRepository.Verify(repo => repo.GetUserByIdAsync(userId), Times.Once());
            _mockUserRepository.Verify(repo => repo.GetPostsByUserIdAsync(It.IsAny<string>()), Times.Never());
        }

    }
}


