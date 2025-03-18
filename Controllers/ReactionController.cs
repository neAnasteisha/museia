using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.IService;
using museia.Models;
using museia.Services;
using System.Security.Claims;

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

        [HttpGet]
        public IActionResult AddReaction(uint postId)
        {
            ViewBag.PostId = postId;
            return View("~/Views/Reaction/AddReaction.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> AddReaction(uint postId, Emoji reactionType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                ModelState.AddModelError(string.Empty, "User is not authenticated.");
                return View("~/Views/Reaction/AddReaction.cshtml");
            }

            try
            {
                await _reactionService.AddReactionAsync(reactionType, userId, postId);
                return RedirectToAction("Index", "Post");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View("~/Views/Reaction/AddReaction.cshtml");
            }
        }
    }
}
