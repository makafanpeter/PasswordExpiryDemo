using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.Infrastructure.Services.Identity
{
    public interface ICurrentUserService
    {
        string GetUserIdentity();
        string GetUserName();
        bool IsAuthenticated { get; }
        ApplicationUser CurrentUser { get; set; }

    }




}
