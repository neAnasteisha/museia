namespace museia.Tests
{
    using Microsoft.EntityFrameworkCore;
    using museia.Data;

    public class InMemoryDB
    {
        public static DbContextOptions<AppDbContext> CreateInMemoryDbOptions()
        {
            return new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }
    }
}
