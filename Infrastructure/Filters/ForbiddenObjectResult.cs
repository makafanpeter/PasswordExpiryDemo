using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PasswordPoliciesDemo.API.Infrastructure.Filters
{
    public class ForbiddenObjectResult : ObjectResult
    {
        public ForbiddenObjectResult(object error)
            : base(error)
        {
            StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}