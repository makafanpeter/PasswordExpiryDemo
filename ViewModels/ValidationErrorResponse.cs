using System.Collections.Generic;

namespace PasswordPoliciesDemo.API.ViewModels
{
    public class ValidationErrorResponse : ErrorResponse
    {
        public IDictionary<string, string[]> Errors { get; set; }
    }
}