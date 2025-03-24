namespace museia.Services
{
    using Microsoft.AspNetCore.Identity;
    using museia.Models;
    using System.Security.Claims;

    public class CurrentUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user != null ? await _userManager.GetUserAsync(user) : null;
        }

        public async Task<UserType?> GetCurrentUserTypeAsync()
        {
            var currentUser = await GetCurrentUserAsync();
            return currentUser?.UserType;
        }

        public async Task RefreshCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId != null)
            {
                var currentUser = await _userManager.FindByIdAsync(userId);
            }
        }

    }
}

