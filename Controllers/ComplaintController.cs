using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using museia.IService;
using museia.Models;
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

        [HttpGet]
        public async Task<IActionResult> Complaints()
        {
            var complaintViewModels = await _complaintService.GetAllUnconsideredComplaintsAsync();
            return View(complaintViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateComplaintStatus(uint complaintId, string action, string userId)
        {
            bool result = false;

            if (action == "approve")
            {
                result = await _complaintService.ApproveComplaint(complaintId);
                var acceptedComplaintsCount = _complaintService.GetAcceptedComplaintsCountForUser(userId);
                ViewBag.AcceptedComplaintsCount = acceptedComplaintsCount;
            }
            else if (action == "reject")
            {
                result = await _complaintService.RejectComplaint(complaintId);
            }

            if (result)
            {
                var updatedComplaints = await _complaintService.GetAllUnconsideredComplaintsAsync();
                return View("Complaints", updatedComplaints);
            }

            return BadRequest("Не вдалося оновити статус.");
        }
    }
}
