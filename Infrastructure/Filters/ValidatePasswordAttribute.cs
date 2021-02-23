using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;
using PasswordPoliciesDemo.API.ViewModels;

namespace PasswordPoliciesDemo.API.Infrastructure.Filters
{
    public sealed class ValidatePasswordAttribute : TypeFilterAttribute
    {
        #region Ctor

        /// <summary>
        /// Create instance of the filter attribute
        /// </summary>
        public ValidatePasswordAttribute() : base(typeof(ValidatePasswordFilter))
        {
        }

        #endregion

        #region Nested filter

        /// <summary>
        /// Represents a filter that validates customer password expiration
        /// </summary>
        private class ValidatePasswordFilter : IAsyncActionFilter
        {
            #region Fields

            private readonly IUserManager _userManager;
            private readonly ICurrentUserService _currentUserService;

            #endregion

            #region Ctor

            public ValidatePasswordFilter(IUserManager userManager,
                ICurrentUserService currentUserService)
            {
                _userManager = userManager;
                _currentUserService = currentUserService;
            }

            #endregion

            #region Utilities

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <returns>A task that on completion indicates the necessary filter actions have been executed</returns>
            private async Task ValidatePasswordAsync(ActionExecutingContext context)
            {
                if (context == null)
                    throw new ArgumentNullException(nameof(context));

                if (context.HttpContext.Request == null)
                    return;


                //get action and controller names
                var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var actionName = actionDescriptor?.ActionName;
                var controllerName = actionDescriptor?.ControllerName;

                if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                    return;

                //don't validate on the 'Change Password' endpoint
                if (controllerName.Equals("Identity", StringComparison.InvariantCultureIgnoreCase) &&
                    actionName.Equals("ChangePassword", StringComparison.InvariantCultureIgnoreCase))
                    return;

                //don't validate on the 'Login' endpoint
                if (controllerName.Equals("Identity", StringComparison.InvariantCultureIgnoreCase) &&
                    actionName.Equals("Login", StringComparison.InvariantCultureIgnoreCase))
                    return;

                //check password expiration
                var user =  _currentUserService.CurrentUser;
                if (user == null)
                   return;
                
                if (!await _userManager.HasPasswordExpiredAsync(user))
                    return;

                //return an error
                var response = new ErrorResponse()
                {
                    Code = "PasswordExpired",
                    Message = $"{_currentUserService.GetUserName()} Requires Password Change"
                };
                context.Result = new ForbiddenObjectResult(response);

            }

            #endregion

            #region Methods

            /// <summary>
            /// Called asynchronously before the action, after model binding is complete.
            /// </summary>
            /// <param name="context">A context for action filters</param>
            /// <param name="next">A delegate invoked to execute the next action filter or the action itself</param>
            /// <returns>A task that on completion indicates the filter has executed</returns>
            public async Task OnActionExecutionAsync(ActionExecutingContext context, 
                ActionExecutionDelegate next)
            {
                await ValidatePasswordAsync(context);
                if (context.Result == null)
                    await next();
            }

            #endregion
        }

        #endregion
    }
}
