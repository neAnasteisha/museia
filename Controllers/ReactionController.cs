using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using museia.Data;
using museia.IService;
using museia.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace museia.Controllers
{
    public class ReactionController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IReactionService _reactionService;

        public ReactionController(AppDbContext context, IReactionService reactionService)
        {
            _context = context;
            _reactionService = reactionService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReaction(uint postId, Emoji reactionType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            await _reactionService.AddReactionAsync(reactionType, userId, postId);

            var reactions = await _context.Reactions
                .Where(r => r.PostID == postId)
                .ToListAsync();

            var counts = reactions
                .GroupBy(r => r.ReactionType)
                .ToDictionary(
                    g => ((int)g.Key).ToString(),
                    g => g.Count()
                );

            var my = reactions.FirstOrDefault(r => r.UserID == userId)?.ReactionType;
            int? myReaction = my.HasValue ? (int?)my.Value : null;

            return Json(new
            {
                PostId = postId,
                Counts = counts,
                MyReaction = myReaction
            });
        }
    }
}
