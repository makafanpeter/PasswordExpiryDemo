namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class SystemErrorException : DemoException
    {
        public SystemErrorException(string message) : base(message, "SYSTEM_ERROR")
        {

        }
        public SystemErrorException() : base("An Unexpected error occured please try again or confirm current operation status", "SYSTEM_ERROR")
        {

        }
    }
}