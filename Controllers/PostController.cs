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
        public async Task<IActionResult> CreatePost(Post model, IFormFile photoFile)
        {
            string photoPath = null;

            if (photoFile != null && photoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                Directory.CreateDirectory(uploadsFolder); 
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(stream);
                }

                photoPath = "/uploads/" + uniqueFileName; 
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier).Value;

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

            // Якщо і опис, і фото не заповнені, додаємо помилку в ModelState
            if (string.IsNullOrWhiteSpace(model.PostText) && model.PostPhoto == null)
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
            post.PostTag = model.PostTag.Value;

            if (model.PostPhoto != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/posts", post.UserID);
                Directory.CreateDirectory(uploadsFolder);

                // Видалення старого фото
                if (!string.IsNullOrEmpty(post.PostPhoto))
                {
                    string oldImagePath = Path.Combine("wwwroot", post.PostPhoto.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                string uniqueFileName = $"post_{Guid.NewGuid()}{Path.GetExtension(model.PostPhoto.FileName)}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PostPhoto.CopyToAsync(stream);
                }

                post.PostPhoto = $"/uploads/posts/{post.UserID}/{uniqueFileName}";
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