using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.IService;
using museia.Models;
using museia.Services;
using NuGet.Protocol.Plugins;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace museia.Controllers
{
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IPostService _postService;
        private readonly IComplaintService _complaintService;

        public PostController(AppDbContext context, UserManager<User> userManager, IPostService postService, IComplaintService complaintService)
        {
            _context = context;
            _userManager = userManager;
            _postService = postService;
            _complaintService = complaintService;
        }


        public async Task<IActionResult> Index(string searchText)
        {
            List<Post> posts = await _postService.SearchPostsAsync(searchText);
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var userPosts = await _postService.GetPostsOfUserAsync(currentUserId);
            bool hasActiveComplaint = false;
            foreach (var post in userPosts)
            {
                var complaints = await _complaintService.GetComplaintsByPostIdAsync(post.PostID);
                if (complaints.Any(c => c.ComplaintStatus == ComplaintStatus.Accepted && !c.IsAcknowledged))
                {
                    hasActiveComplaint = true;
                    break;
                }
            }

            if (hasActiveComplaint)
            {
                return RedirectToAction("WarningView", "Complaint");
            }

            return View(posts);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Rules()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            ViewBag.PostTags = _postService.GetPostTags();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(Post model, string photoCropped)
        {
            string photoPath = null;

            if (!string.IsNullOrEmpty(photoCropped))
            {
                try
                {
                    var base64Data = Regex.Match(photoCropped, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                    var imageBytes = Convert.FromBase64String(base64Data);

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/posts");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + ".png";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                    photoPath = "/uploads/posts/" + uniqueFileName;
                }
                catch
                {
                    ModelState.AddModelError("", "Помилка при обробці зображення.");
                    return View(model);
                }
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            await _postService.CreatePostAsync(model.PostText, photoPath, model.PostTag, userId);

            return RedirectToAction("Index", "Post");
        }



        [HttpGet]
        [Authorize]
        public async Task<IActionResult> EditPost(uint id)
        {
            var post = await _postService.GetPostById(id);
            if (post == null || post.UserID != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            var model = new EditPostViewModel
            {
                PostID = post.PostID,
                PostText = post.PostText,
                PostPhotoUrl = post.PostPhoto,
                PostTag = post.PostTag
            };
            ViewBag.PostTags = _postService.GetPostTags();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> EditPost(EditPostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.PostTags = _postService.GetPostTags();
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.PostText) && string.IsNullOrWhiteSpace(model.PostPhotoCropped))
            {
                ModelState.AddModelError("", "Ви повинні заповнити принаймні одне з полів: опис або фото.");
                ViewBag.PostTags = _postService.GetPostTags();
                return View(model);
            }

            var post = await _postService.GetPostById(model.PostID);
            if (post == null || post.UserID != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            post.PostText = model.PostText;
            post.PostTag = model.PostTag ?? post.PostTag;
            post.EditedAt = DateTime.Now;

            // Якщо є обрізане фото (base64)
            if (!string.IsNullOrEmpty(model.PostPhotoCropped))
            {
                var base64Data = Regex.Match(model.PostPhotoCropped, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/posts", post.UserID);
                Directory.CreateDirectory(uploadsFolder);

                // Видалити старе фото
                if (!string.IsNullOrEmpty(post.PostPhoto))
                {
                    string oldImagePath = Path.Combine("wwwroot", post.PostPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                string fileName = $"post_{Guid.NewGuid()}.png";
                string filePath = Path.Combine(uploadsFolder, fileName);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                post.PostPhoto = $"/uploads/posts/{post.UserID}/{fileName}";
            }

            await _postService.UpdatePost(post);
            return RedirectToAction("Profile", "User");
        }





        [HttpPost]
        public async Task<IActionResult> DeletePost(uint id, UserType userType)
        {
            await _postService.DeletePost(id);
            if(userType == UserType.SimpleUser)
            {
                return RedirectToAction("Profile", "User");
            }
            else
            {
                return RedirectToAction("Complaints", "Complaint");
            }
        }



    }
}