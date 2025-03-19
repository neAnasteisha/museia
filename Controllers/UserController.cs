using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.Models;
using museia.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace museia.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ComplaintService _complaintService;
        private readonly PostService _postService;
        private readonly ReactionService _reactionService;

        public UserController(AppDbContext context, UserManager<User> userManager, ComplaintService complaintService, PostService postService, ReactionService reactionService)
        {
            _context = context;
            _userManager = userManager;
            _complaintService = complaintService;
            _postService = postService;
            _reactionService = reactionService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> CreateComplaint(uint postId, string complaintText)
        {
            var userId = _userManager.GetUserId(User);
            await _complaintService.CreateComplaintAsync(complaintText, userId, postId);
            return RedirectToAction("Index", "Post");
        }

        public async Task<IActionResult> AddReaction(Emoji reactionType, uint postId)
        {
            var userId = _userManager.GetUserId(User);
            await _reactionService.AddReactionAsync(reactionType, userId, postId);
            return RedirectToAction("Index", "Post");
        }
        
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var userPosts = await _postService.GetPostsOfUserAsync(userId);

            var model = new ProfileViewModel
            {
                User = user,
                UserPosts = userPosts
            };

            return View(model);
        }
    }
}
