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
        private readonly IComplaintService _complaintService;
        private readonly IPostService _postService;


        public ComplaintController(IComplaintService complaintService, IPostService postService)
        {
            _complaintService = complaintService;
            _postService = postService;
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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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


        [HttpGet]
        public async Task<IActionResult> WarningView()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userPosts = await _postService.GetPostsOfUserAsync(currentUserId);

            Complaint complaintEntity = null;
            foreach (var post in userPosts)
            {
                var complaints = await _complaintService.GetComplaintsByPostIdAsync(post.PostID);
                complaintEntity = complaints.FirstOrDefault(c =>
                    (c.ComplaintStatus == ComplaintStatus.Processing || c.ComplaintStatus == ComplaintStatus.Accepted)
                    && !c.IsAcknowledged);
                if (complaintEntity != null)
                {
                    break;
                }
            }

            if (complaintEntity == null)
            {
                return RedirectToAction("Index", "Post");
            }

            var viewModel = new ComplaintViewModel
            {
                ComplaintID = complaintEntity.ComplaintID,
                ComplaintReason = complaintEntity.ComplaintReason,
                ComplaintStatus = complaintEntity.ComplaintStatus,
                PostId = complaintEntity.PostID,
                PostText = complaintEntity.Post?.PostText,
                PostTag = complaintEntity.Post?.PostTag.ToString(),
                PostPhoto = complaintEntity.Post?.PostPhoto,
                CreatedAt = complaintEntity.Post?.CreatedAt ?? DateTime.MinValue,
                PostsUserId = complaintEntity.Post?.UserID,
                UserCountOfWarnings = await _complaintService.GetAcceptedComplaintsCountForUser(complaintEntity.Post?.UserID)
            };

            return View(viewModel);
        }



        [HttpPost]
        public async Task<IActionResult> AcknowledgeComplaint(uint complaintId)
        {
            var complaint = await _complaintService.GetComplaintByIdAsync(complaintId);
            if (complaint != null)
            {
                complaint.IsAcknowledged = true;
                await _complaintService.UpdateComplaintAsync(complaint);
            }
            return RedirectToAction("Index", "Post");
        }
    }
}
