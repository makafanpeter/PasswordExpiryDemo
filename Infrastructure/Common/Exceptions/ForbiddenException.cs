namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class ForbiddenException : DemoException
    {
        public ForbiddenException(string user) : base($"UnauthorizedAccess: {user} is not allowed to access this resource.", "Access Denied")
        {

        }
        
        public ForbiddenException() : base($"UnauthorizedAccess: user is not allowed to access this resource.", "Access Denied")
        {

        }
    }
}