using FluentValidation;
using PasswordPoliciesDemo.API.Infrastructure.Common;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;

namespace PasswordPoliciesDemo.API.ViewModels.Validations
{
    public class ChangePasswordValidation : NullReferenceAbstractValidator<ChangePasswordRequest>
    {
        private readonly IUserManager _userManager;

        public ChangePasswordValidation(IUserManager userManager)
        {
            _userManager = userManager;
            RuleFor(x => x).NotNull();
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.OldPassword).NotEqual(customer => customer.NewPassword)
                .WithMessage("Old Password must not be the same as New Password");
            RuleFor(x => x.NewPassword).NotEmpty().Must(ValidPassword).WithMessage(x =>
                GetErrorMessage(_userManager.ValidatePasswordRequirement(x.NewPassword)));

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
