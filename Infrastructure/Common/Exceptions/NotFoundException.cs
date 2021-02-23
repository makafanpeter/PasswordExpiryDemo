namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class NotFoundException : DemoException
    {
        public NotFoundException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.", "Not Found")
        {
            
        }

        public NotFoundException(string message) : base(message, "Not Found")
        {

        }

    }
}