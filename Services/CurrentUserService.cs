namespace museia.Services
{
    using Microsoft.AspNetCore.Identity;
    using museia.Models;

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
    }
}

