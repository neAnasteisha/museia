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

        public async Task<IActionResult> Index()
        {
            var posts = await _postService.GetAllPostsAsync();
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
