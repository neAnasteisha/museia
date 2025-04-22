using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using museia.Controllers;
using museia.Models;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace museia.Tests
{
    public class AuthenticationTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly AccountController _accountController;

        public AuthenticationTests()
        {
            _mockUserManager = new Mock<UserManager<User>>(
                new Mock<IUserStore<User>>().Object, null, null, null, null, null, null, null, null
            );

            _mockSignInManager = new Mock<SignInManager<User>>(
                _mockUserManager.Object,
                new Mock<IHttpContextAccessor>().Object,
                new Mock<IUserClaimsPrincipalFactory<User>>().Object,
                null, null, null, null
            );

            _accountController = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnError_WhenUserAlreadyExists()
        {
            string email = "test@example.com";
            _mockUserManager.Setup(u => u.FindByEmailAsync(email))
                            .ReturnsAsync(new User { Email = email });

            var result = await _accountController.Register("username", email, "Password123!", null, null) as ViewResult;

            Assert.NotNull(result);
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Contains("", result.ViewData.ModelState.Keys);
        }

        [Fact]
        public async Task Register_ShouldAddUser()
        {
            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                            .ReturnsAsync((User)null);

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager.Setup(s => s.SignInAsync(It.IsAny<User>(), false, null))
                              .Returns(Task.CompletedTask);

            var result = await _accountController.Register("username", "new@example.com", "Password123!", "Password123!", null) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Post", result.ControllerName);

            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockSignInManager.Verify(sm => sm.SignInAsync(It.IsAny<User>(), false, null), Times.Once);
        }

        [Fact]
        public async Task Logout_ShouldSignOutAndRedirectToLogin()
        {
            _mockSignInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            var result = await _accountController.Logout();

            _mockSignInManager.Verify(s => s.SignOutAsync(), Times.Once);
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        // Тест для перевірки реєстрації користувача з автоматичним погодженням правил
        [Fact]
        public async Task Register_ShouldProceed_WhenUserRegistersAndRulesAreAutomaticallyAccepted()
        {
            
            string email = "newuser@example.com";

            _mockUserManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>()))
                            .ReturnsAsync((User)null);

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                            .ReturnsAsync(IdentityResult.Success);

            _mockSignInManager.Setup(s => s.SignInAsync(It.IsAny<User>(), false, null))
                              .Returns(Task.CompletedTask);

            
            var result = await _accountController.Register("username", email, "Password123!", "Password123!", null) as RedirectToActionResult;

           
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Post", result.ControllerName);

            _mockUserManager.Verify(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);
            _mockSignInManager.Verify(sm => sm.SignInAsync(It.IsAny<User>(), false, null), Times.Once);
        }

        [Fact]
        public async Task Login_ShouldLogInAndRedirectToHome_WhenCredentialsAreCorrect()
        {
            var user = new User
            {
                Email = "supermario@mail.com"
            };

            _mockUserManager.Setup(u => u.FindByEmailAsync(user.Email))
                            .ReturnsAsync(user);
            _mockSignInManager.Setup(s => s.PasswordSignInAsync(user, "password", false, false))
                              .ReturnsAsync(SignInResult.Success);

            var result = await _accountController.Login(user.Email, "password") as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal("Post", result.ControllerName);
        }

        [Fact]
        public async Task Login_ShouldReturnError_WhenCredentialsAreIncorrect()
        {
            var email = "unrealuser@e.mail";
            var password = "wrongpassword";

            _mockUserManager.Setup(u => u.FindByEmailAsync(email))
                            .ReturnsAsync((User)null);

            _mockSignInManager.Setup(s => s.PasswordSignInAsync(It.IsAny<User>(), password, false, false))
                              .ReturnsAsync(SignInResult.Failed);

            var result = await _accountController.Login(email, password) as ViewResult;

            Assert.NotNull(result);
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Contains("", result.ViewData.ModelState.Keys);
        }
    }
}
