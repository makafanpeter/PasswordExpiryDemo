using FluentValidation;
using PasswordPoliciesDemo.API.Infrastructure.Common;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;

namespace PasswordPoliciesDemo.API.ViewModels.Validations
{
    public class RegisterRequestValidation : NullReferenceAbstractValidator<RegisterRequest>
    {
        private readonly IUserManager _userManager;

        public RegisterRequestValidation(IUserManager userManager)
        {
            _userManager = userManager;
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.FirstName).NotNull();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Password).NotEmpty().Must(ValidPassword).WithMessage(x =>
                GetErrorMessage(_userManager.ValidatePasswordRequirement(x.Password)));

        }

        private static string GetErrorMessage(SecurityResult result)
        {

            return string.Join(",", result.Errors);
        }

        private bool ValidPassword(string password)
        {

            var result = _userManager.ValidatePasswordRequirement(password);

            return result.Succeeded;
        }
    }
}