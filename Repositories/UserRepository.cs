namespace museia.Repositories
{
    using museia.Data;

    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }
    }
}
