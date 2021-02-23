using FluentValidation;
using PasswordPoliciesDemo.API.Infrastructure.Common;

namespace PasswordPoliciesDemo.API.ViewModels.Validations
{
    public class LoginValidation : NullReferenceAbstractValidator<LoginRequest>
    {

        public LoginValidation()
        {
            RuleFor(x => x).NotNull();
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.TokenExpireAt).GreaterThan(0).When(x => x.TokenExpireAt.HasValue);
        }

    }
}