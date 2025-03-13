using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Data;
using museia.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace museia.Controllers
{
    [Authorize] // ✅ Доступ тільки для залогінених користувачів
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public PostController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Відображення форми створення поста
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Post model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Отримуємо поточного користувача
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Створюємо новий пост
            var post = new Post
            {
                PostText = model.PostText,
                PostTag = model.PostTag,
                CreatedAt = DateTime.UtcNow,
                UserID = user.Id
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
