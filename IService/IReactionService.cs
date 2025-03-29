using museia.Models;
using museia.Services;

namespace museia.IService
{
    public interface IReactionService
    {
        Task AddReactionAsync(Emoji reactionType, string userId, uint postId);
        Task<List<Reaction>> GetReactionsByUserIdAsync(string userId);
        Task DeleteReactionAsync(uint id);
    }
}
