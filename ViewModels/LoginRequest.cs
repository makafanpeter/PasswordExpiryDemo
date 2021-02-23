namespace PasswordPoliciesDemo.API.ViewModels
{
    public class LoginRequest
    {

        public string Username { get; set; }

        public string Password { get; set; }

        public int? TokenExpireAt { get; set; }
    }
}