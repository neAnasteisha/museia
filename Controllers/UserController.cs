using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.IService;
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
        private readonly IComplaintService _complaintService;
        private readonly IPostService _postService;
        private readonly IReactionService _reactionService;
        private readonly IUserService _userService;

        public UserController(AppDbContext context, UserManager<User> userManager, IComplaintService complaintService, IPostService postService, IReactionService reactionService, IUserService userService)
        {
            _context = context;
            _userManager = userManager;
            _complaintService = complaintService;
            _postService = postService;
            _reactionService = reactionService;
            _userService = userService;
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

        public async Task<IActionResult> OtherUserProfile(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var profileViewModel = await _userService.GetProfileViewModelByIdAsync(id);
            if (profileViewModel == null)
                return NotFound();

            return View(profileViewModel); 
        }

        [HttpPost]
        public async Task<IActionResult> SendWarning(uint complaintId, uint postId, string postsUserId)
        {
            await _complaintService.AcceptComplaint(complaintId);
            await _postService.DeletePost(postId);
            return RedirectToAction("Complaints", "Complaint");
        }


        public async Task<IActionResult> BlockUser(uint complaintId, uint postId)
        {
            
            string userId = await _postService.GetUserIdByPostIdAsync(postId);
            userId = userId.ToString();
            await _userService.BlockUserAsync(userId);

            var usersPosts = await _postService.GetPostsOfUserAsync(userId);
            foreach (var post in usersPosts)
            {
                await _postService.DeletePost(post.PostID);
            }

            var usersReactions = await _reactionService.GetReactionsByUserIdAsync(userId);
            foreach (var reaction in usersReactions)
            {
                await _reactionService.DeleteReactionAsync(reaction.ReactionID);
            }

            var usersComplaints = await _complaintService.GetComplaintsByUserIdAsync(userId);
            foreach (var complaint in usersComplaints)
            {
                await _complaintService.DeleteComplaintAsync(complaint.ComplaintID);
            }

            return RedirectToAction("Complaints", "Complaint");
        }
    }

}
