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
        private readonly UserService _userService;

        public UserController(AppDbContext context, UserManager<User> userManager, ComplaintService complaintService, PostService postService, ReactionService reactionService, UserService userService)
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

        public IActionResult SendWarning(uint complaintId, uint postId, string postsUserId)
        {
            // Оновлюємо статус скарги через сервіс
            _complaintService.AcceptComplaint(complaintId);
            _postService.DeletePost(postId);


            // Перевірка, чи було показано попередження
            //if (HttpContext.Session.GetString("ComplaintMessageShown") == null)
            //{
            //    // Встановлюємо прапор у сесії, щоб не показувати повідомлення більше
            //    HttpContext.Session.SetString("ComplaintMessageShown", "true");

            //    // Відправляємо повідомлення користувачеві
            //    TempData["Message"] = "Ваш допис порушує правила нашого сервісу, тож був видалений. Якщо у Вас буде більше 3 скарг Ваш профіль буде заблоковано.";
            //}

            return RedirectToAction("Complaints", "Complaint"); // або на іншу сторінку
        }

        public async Task<IActionResult> BlockUser(uint complaintId, uint postId)
        {
            
            string userId = await _postService.GetUserIdByPostIdAsync(postId);
            userId = userId.ToString();
            await _userService.DeleteUserAsync(userId);

            return RedirectToAction("Complaints", "Complaint");
        }
    }

}
