namespace museia.Tests
{
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
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

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

    }
}
