using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Models;
using System.Threading.Tasks;

namespace museia.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Показує сторінку входу
        public IActionResult Login()
        {
            return View();
        }

        // Обробка входу
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неправильний email або пароль.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Невдала спроба входу.");
            return View();
        }

        // Показує сторінку реєстрації
        public IActionResult Register()
        {
            return View();
        }

        // Обробка реєстрації
        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password)
        {
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("", "Користувач із таким email вже існує!");
                return View();
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                UserType = UserType.SimpleUser
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        // Вихід з акаунта
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
