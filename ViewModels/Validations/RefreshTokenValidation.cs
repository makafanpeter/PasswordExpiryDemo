using FluentValidation;
using PasswordPoliciesDemo.API.Infrastructure.Common;

namespace PasswordPoliciesDemo.API.ViewModels.Validations
{
    public class RefreshTokenValidation : NullReferenceAbstractValidator<RefreshTokenRequest>
    {

        public RefreshTokenValidation()
        {
            RuleFor(x => x).NotNull();

            RuleFor(x => x.Token).NotEmpty();

        }
    }
}