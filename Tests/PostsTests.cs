namespace museia.Tests
{
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using museia.Data;
    using museia.Models;
    using museia.Services;
    using Xunit;

    public class PostsTests
    {
        [Fact]
        public async Task SearchPostsByTagAsync_ShouldReturnPostsWithSomeTag()
        {
            var options = InMemoryDB.CreateInMemoryDbOptions();

            using (var context = new AppDbContext(options))
            {
                context.Posts.Add(new Post { PostID = 1, PostTag = PostTag.Поезія, PostText = "Test Post 1", UserID = "user1" });
                context.Posts.Add(new Post { PostID = 2, PostTag = PostTag.Фото, PostText = "Test Post 2", UserID = "user2" });
                context.Posts.Add(new Post { PostID = 3, PostTag = PostTag.Поезія, PostText = "Test Post 3", UserID = "user3" });
                await context.SaveChangesAsync();
            }

            using (var context = new AppDbContext(options))
            {
                var repository = new PostRepository(context);
                var service = new PostService(repository);

                var result = await service.SearchPostsAsync("#Поезія");

                Assert.NotNull(result);
                Assert.Equal(2, result.Count);
                Assert.All(result, post => Assert.Equal(PostTag.Поезія, post.PostTag));
            }
        }

        private async Task<(AppDbContext context, PostService postService, User user)> CreateTestContext()
        {
            var options = InMemoryDB.CreateInMemoryDbOptions();

            var context = new AppDbContext(options);
            var postRepository = new PostRepository(context);
            var postService = new PostService(postRepository);

            var user = new User { UserName = "testuser", Email = "testuser@example.com" };

            var userManager = new UserManager<User>(
                new UserStore<User>(context),
                null, null, null, null, null, null, null, null);

            await userManager.CreateAsync(user);
            await context.SaveChangesAsync();

            return (context, postService, user);
        }

        [Fact]
        public async Task CreatePostAsync_ShouldAddPost_WhenTextIsProvided()
        {
            var (context, postService, user) = await CreateTestContext();
            
            var postText = "Це тестовий допис";
            var postTag = PostTag.Малюнок;
            var userId = user.Id;

            await postService.CreatePostAsync(postText, null, postTag, userId);

            var post = await context.Posts.FirstOrDefaultAsync();
            Assert.NotNull(post);
            Assert.Equal(postText, post.PostText);
            Assert.Null(post.PostPhoto);
            Assert.Equal(postTag, post.PostTag);
            Assert.Equal(userId, post.UserID);
            
        }

        [Fact]
        public async Task CreatePostAsync_ShouldAddPost_WhenPhotoIsProvided()
        {
            var options = InMemoryDB.CreateInMemoryDbOptions();

            var (context, postService, user) = await CreateTestContext();

            var postPhoto = "/uploads/test.jpg";
            var postTag = PostTag.Малюнок;
            var userId = user.Id;

            await postService.CreatePostAsync(null, postPhoto, postTag, userId);

            var post = await context.Posts.FirstOrDefaultAsync();
            Assert.NotNull(post);
            Assert.Null(post.PostText);
            Assert.Equal(postPhoto, post.PostPhoto);
            Assert.Equal(postTag, post.PostTag);
            Assert.Equal(userId, post.UserID);
            
        }

        [Fact]
        public async Task CreatePostAsync_ShouldThrowException_WhenPostIsEmpty()
        {
            var options = InMemoryDB.CreateInMemoryDbOptions();
            var (context, postService, user) = await CreateTestContext();

            var userId = user.Id;

            var exception = await Assert.ThrowsAsync<ArgumentException>(
                () => postService.CreatePostAsync(null, null, PostTag.Музика, userId));

            Assert.Equal("Допис не може бути порожнім.", exception.Message);
        }
    }
}
