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

        // Відображення форми створення поста
        public IActionResult Create()
        {
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
    }
}
