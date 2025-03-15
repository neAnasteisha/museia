namespace museia.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using museia.Models;
    using museia.Services;
    using System.Diagnostics;

    public class HomeController : Controller
    {
        private readonly PostService _postService;

        public HomeController(PostService postService)
        {
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
    }
}
