namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class UnAuthorizedException : DemoException
    {
        public UnAuthorizedException(string message ):base(message, "UnAuthorized")
        {

        }
    }
}