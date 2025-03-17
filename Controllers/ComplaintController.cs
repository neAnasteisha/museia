using Microsoft.AspNetCore.Mvc;
using museia.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace museia.Controllers
{
    public class ComplaintController : Controller
    {
        private readonly ComplaintService _complaintService;

        public ComplaintController(ComplaintService complaintService)
        {
            _complaintService = complaintService;
        }

        [HttpGet]
        public IActionResult Report(int postId)
        {
            ViewBag.PostId = postId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Report(int postId, string complaintReason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the current user's ID

            try
            {
                await _complaintService.CreateComplaintAsync(complaintReason, userId, (uint)postId);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View();
            }

            return RedirectToAction("Index", "Post");
        }
    }
}
