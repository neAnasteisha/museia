﻿namespace museia.Tests
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
    }
}
