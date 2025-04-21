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

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError("", "Неправильний email або пароль.");
                return View();
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError("", "Ваш обліковий запис заблокований.");
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Post");
            }

            ModelState.AddModelError("", "Невдала спроба входу.");
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, IFormFile avatar)
        {
            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                if (existingUser.IsBlocked)
                {
                    ModelState.AddModelError("", "Ваш обліковий запис заблокований. Ви не можете зареєструватися повторно.");
                    return View();
                }

                ModelState.AddModelError("", "Користувач із таким email вже існує!");
                return View();
            }

            if(password != confirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають!");
                return View();
            }

            string avatarPath = "/uploads/avatar.jpg";

            if(avatar != null && avatar.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(avatar.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await avatar.CopyToAsync(stream);
                }

                avatarPath = "/uploads/avatars/" + uniqueFileName;
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                UserAvatar = avatarPath,
                UserType = UserType.SimpleUser
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Post");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Username = user.UserName,
                Description = user.UserDescription,
                AvatarUrl = user.UserAvatar
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            user.UserName = model.Username;
            user.UserDescription = model.Description;

            if (model.Avatar != null && model.Avatar.Length > 0)
            {
                // Створюємо шлях для аватара користувача
                string userFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars", user.Id);
                string avatarFolder = Path.Combine(userFolder, "avatar");

                // Очищуємо папку avatar
                if (Directory.Exists(avatarFolder))
                {
                    foreach (var file in Directory.GetFiles(avatarFolder))
                    {
                        System.IO.File.Delete(file);
                    }
                }
                else
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                // Генеруємо унікальне ім'я файлу
                string fileExtension = Path.GetExtension(model.Avatar.FileName);
                string uniqueFileName = $"avatar{fileExtension}"; // avatar.png / avatar.jpg
                string filePath = Path.Combine(avatarFolder, uniqueFileName);

                // Зберігаємо новий файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }

                // Оновлюємо шлях у базі
                user.UserAvatar = $"/uploads/avatars/{user.Id}/avatar/{uniqueFileName}";
            }

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "User");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProfile()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return RedirectToAction("EditProfile");
            }

            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Account");
        }
    }
}
