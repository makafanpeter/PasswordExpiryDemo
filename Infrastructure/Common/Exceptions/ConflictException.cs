using System;

namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class ConflictException : DemoException
    {
        public ConflictException(Exception ex):base("The Inventory has been modified by another user. Please get the Inventory to get its current values",ex)
        {
            
        }
    }
}
