namespace PasswordPoliciesDemo.API.Infrastructure.Application.Configurations
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        public int ExpiryMinutes { get; set; }
        public string Issuer { get; set; }
        public bool ValidateLifetime { get; set; }
        public bool ValidateIssuerSigningKey { get; set; }
        public bool ValidateAudience { get; set; }
    }
}