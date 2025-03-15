using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.Models;
using museia.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace museia.Controllers
{
    [Authorize] // ✅ Доступ тільки для залогінених користувачів
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly PostService _postService;

        public PostController(AppDbContext context, UserManager<User> userManager, PostService postService)
        {
            _context = context;
            _userManager = userManager;
            _postService = postService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.PostTags = _postService.GetPostTags();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Post post)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found");
            }
            // Викликаємо сервіс для створення поста
            await _postService.CreatePostAsync(post.PostText, post.PostPhoto, post.PostTag, userId);

            return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
            }
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(uint id)
        {
            await _postService.DeletePost(id);
            return RedirectToAction("Index", "Home");
        }
    }
}
