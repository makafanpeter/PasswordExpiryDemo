namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class BadRequestException : DemoException
    {
        public BadRequestException(string message) : base(message, "Invalid Request")
        {
            
        }
    }
}