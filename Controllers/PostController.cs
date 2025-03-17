﻿using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.Models;
using museia.Services;
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
        private readonly PostService _postService;
        private readonly ComplaintService _complaintService;

        public PostController(AppDbContext context, UserManager<User> userManager, PostService postService)
        {
            _context = context;
            _userManager = userManager;
            _postService = postService;
        }

        public async Task<IActionResult> Index(string searchText)
        {
            List<Post> posts;
            posts = await _postService.SearchPostsAsync(searchText);

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
        public IActionResult Create()
        {
            ViewBag.PostTags = _postService.GetPostTags();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Post model, IFormFile photoFile)
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
        public async Task<IActionResult> Edit(uint id)
        {
            var post = await _postService.GetPostById(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewBag.PostTags = _postService.GetPostTags();
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Post post)
        {
            if (ModelState.IsValid)
            {
                await _postService.UpdatePost(post);
                return RedirectToAction("Index", "Post");
            }
            return View(post);
        }

        [HttpPost]
        public IActionResult Report(uint id, string complaintReason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                _complaintService.CreateComplaintAsync(complaintReason, userId, id).Wait();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("ПричинаСкарги", ex.Message);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(uint id)
        {
            await _postService.DeletePost(id);
            return RedirectToAction("Index", "Post");
        }

        [HttpGet]
        public IActionResult AddReaction(uint postId)
        {
            ViewBag.PostId = postId;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddReaction(uint postId, Emoji reactionType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
                await _postService.AddReactionAsync(reactionType, userId, postId);
                return RedirectToAction("Index", "Post");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View();
            }
        }
    }
}