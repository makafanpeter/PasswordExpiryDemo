using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.Infrastructure.Services.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _context;
        private readonly IUserManager _userManager;

        public CurrentUserService(IHttpContextAccessor context, IUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string GetUserIdentity()
        {
            return _context.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
        }

        public string GetUserName()
        {
            return _context.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }


        public ApplicationUser CurrentUser
        {
            get
            {
                if (_cachedUser != null)
                    return _cachedUser;
                if (long.TryParse(GetUserIdentity(), out var userId))
                {
                    var task = Task.Run(async () => await _userManager.FindByIdAsync(userId));
                    var user = task.Result;
                    if (user != null && _userManager.IsUserActiveAsync(user).Result)
                    {
                        _cachedUser = user;
                    }
                }

                return _cachedUser;
            }
            set => _cachedUser = value;
        }


        public bool IsAuthenticated => !string.IsNullOrEmpty(GetUserIdentity());


        private ApplicationUser _cachedUser;

    }
}