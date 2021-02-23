using PasswordPoliciesDemo.API.Infrastructure.Auth.Token;

namespace PasswordPoliciesDemo.API.ViewModels
{
    public class LoginResponse
    {


        public JsonWebToken JsonWebToken { get; set; }

        public UserProfile UserDetails { get; set; }
        public string Status { get; set; }
    }
}