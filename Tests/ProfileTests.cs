namespace museia.Tests
{
    using Microsoft.AspNetCore.Identity;
    using Moq;
    using museia.IRepository;
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
        public async Task DeleteUserAsync_ShouldRemoveUser()
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
    }
}
