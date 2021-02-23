namespace PasswordPoliciesDemo.API.Infrastructure.Application.Configurations
{
    public class ApplicationUserSettings
    {

        public int AccountActivationLinkDaysValid { get; set; }
        public int PasswordRecoveryLinkDaysValid { get; set; }

        public string SecretKey { get; set; }
        public int FailedPasswordAllowedAttempts { get; set; }
        public int FailedPasswordLockoutMinutes { get; set; }
      
        public int PasswordLifetimeDays { get; set; }

        public int PinLength { get; set; }
        public int PasswordMinimumLength { get; set; }

        public bool PasswordRequireUppercase { get; set; }
        public bool PasswordRequireLowercase { get; set; }
        public bool PasswordRequireNonAlphanumeric { get; set; }
        public bool PasswordRequireDigit { get; set; }
    }
}
