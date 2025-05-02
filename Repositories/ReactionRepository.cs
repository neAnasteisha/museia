using Microsoft.EntityFrameworkCore;
using museia.Data;
using museia.IRepository;
using museia.Models;

namespace museia.Repositories
{
    public class ReactionRepository : IReactionRepository
    {
        private readonly AppDbContext _context;

        public ReactionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddReactionAsync(Reaction reaction)
        {
            _context.Reactions.Add(reaction);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrUpdateReactionAsync(Reaction reaction)
        {
            var existingReaction = await _context.Reactions
                .FirstOrDefaultAsync(r => r.PostID == reaction.PostID && r.UserID == reaction.UserID);

            if (existingReaction != null)
            {
                if (existingReaction.ReactionType == reaction.ReactionType)
                {
                    // Якщо реакція така ж сама — видалити
                    _context.Reactions.Remove(existingReaction);
                }
                else
                {
                    // Якщо інша — оновити
                    existingReaction.ReactionType = reaction.ReactionType;
                    _context.Reactions.Update(existingReaction);
                }
            }
            else
            {
                // Якщо реакції ще не було — додати
                _context.Reactions.Add(reaction);
            }

            await _context.SaveChangesAsync();
        }


        public async Task<List<Reaction>> GetReactionsByUserIdAsync(string userId)
        {
            return await _context.Reactions
                .Where(p => p.UserID == userId)
                .ToListAsync();
        }

        public async Task DeleteReactionAsync(uint id)
        {
            var reaction = await _context.Reactions.FindAsync(id);
            if (reaction != null)
            {
                _context.Reactions.Remove(reaction);
                await _context.SaveChangesAsync();
            }
        }
    }
}
