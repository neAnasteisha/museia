using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using museia.Models;
using System.Text.RegularExpressions;
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
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword, string avatarCropped)
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

            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають!");
                return View();
            }

            string avatarPath = "/uploads/avatar.jpg";

            if (!string.IsNullOrEmpty(avatarCropped))
            {
                try
                {
                    var base64Data = Regex.Match(avatarCropped, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                    var imageBytes = Convert.FromBase64String(base64Data);

                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars");
                    Directory.CreateDirectory(uploadsFolder);

                    string uniqueFileName = Guid.NewGuid().ToString() + ".png";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                    avatarPath = "/uploads/avatars/" + uniqueFileName;
                }
                catch
                {
                    ModelState.AddModelError("", "Помилка при обробці зображення.");
                    return View();
                }
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
            if (userId == null) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            user.UserName = model.Username;
            user.UserDescription = model.Description;

            if (!string.IsNullOrEmpty(model.AvatarCropped))
            {
                var base64Data = Regex.Match(model.AvatarCropped, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                var imageBytes = Convert.FromBase64String(base64Data);

                string userFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatars", user.Id);
                string avatarFolder = Path.Combine(userFolder, "avatar");

                if (Directory.Exists(avatarFolder))
                {
                    foreach (var file in Directory.GetFiles(avatarFolder))
                        System.IO.File.Delete(file);
                }
                else
                {
                    Directory.CreateDirectory(avatarFolder);
                }

                string filePath = Path.Combine(avatarFolder, "avatar.png");
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                user.UserAvatar = $"/uploads/avatars/{user.Id}/avatar/avatar.png";
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
