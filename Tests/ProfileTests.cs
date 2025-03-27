namespace museia.Tests
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;
    using museia.Repositories;
    using museia.Services;
    using Xunit;

    public class ProfileTests
    {
        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUserFromDB()
        {
            var options = InMemoryDB.CreateInMemoryDbOptions();

            using (var context = new AppDbContext(options))
            {
                var user = new User { UserName = "testuser", Email = "testuser@example.com" };

                var userManager = new UserManager<User>(
                    new UserStore<User>(context),
                    null, null, null, null, null, null, null, null);

                await userManager.CreateAsync(user);

                var userRepository = new UserRepository(context);
                var userService = new UserService(userRepository);

                await userService.DeleteUserAsync(user.Id);
            }

            using (var context = new AppDbContext(options))
            {
                var user = await context.Users.FindAsync("testuser");
                Assert.Null(user);
            }
        }
    }
}
