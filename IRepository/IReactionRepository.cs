using museia.Models;

namespace museia.IRepository
{
    public interface IReactionRepository
    {
        Task AddOrUpdateReactionAsync(Reaction reaction);
        Task<List<Reaction>> GetReactionsByUserIdAsync(string userId);
        Task DeleteReactionAsync(uint id);
    }
}
