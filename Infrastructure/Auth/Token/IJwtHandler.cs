namespace PasswordPoliciesDemo.API.Infrastructure.Auth.Token
{
    public interface IJwtHandler
    {

        JsonWebToken Create(string username, int? expiryMinutes = 0);

        JwtValidationResult Validate(string token);
    }
}
