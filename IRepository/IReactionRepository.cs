using museia.Models;

namespace museia.IRepository
{
    public interface IReactionRepository
    {
        Task AddOrUpdateReactionAsync(Reaction reaction);
    }
}
