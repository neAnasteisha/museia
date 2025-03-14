namespace museia.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using museia.Models;
    using museia.Services;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;

        //public HomeController(ILogger<HomeController> logger)
        //{
        //    _logger = logger;
        //}
        private readonly PostRepository _postService;

        public HomeController(PostRepository postService)
        {
            _postService = postService;
        }

        public async Task<IActionResult> Index(string searchText)
        {
            List<Post> posts;

            if (!string.IsNullOrEmpty(searchText)) // якщо Ї текст дл€ пошуку
            {
                posts = await _postService.SearchPostsAsync(searchText); // ¬икликаЇмо метод пошуку з серв≥су
            }
            else
            {
                posts = await _postService.GetAllPostsAsync(); // якщо пошук порожн≥й, показуЇмо вс≥ пости
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
    }
}
