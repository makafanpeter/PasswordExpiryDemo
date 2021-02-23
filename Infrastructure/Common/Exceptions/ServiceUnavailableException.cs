namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class ServiceUnavailableException : DemoException
    {
        public ServiceUnavailableException():base("An Unexpected error occured please try again or confirm current operation status", "Service Unavailable")
        {

        }
    }
}