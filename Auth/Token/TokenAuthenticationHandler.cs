using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PasswordPoliciesDemo.API.Infrastructure.Auth.Token;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;
using PasswordPoliciesDemo.API.ViewModels;

namespace PasswordPoliciesDemo.API.Auth.Token
{
    public class TokenAuthenticationHandler : AuthenticationHandler<TokenAuthenticationOptions>
    {
        protected string SignInScheme => Options.SignInScheme;
        private readonly IJwtHandler _jwtHandler;
        private readonly IUserManager _userManager;

        public TokenAuthenticationHandler(IOptionsMonitor<TokenAuthenticationOptions> options,
            ILoggerFactory logger, 
            UrlEncoder encoder,
            ISystemClock clock,
            IJwtHandler jwtHandler,
            IUserManager userManager)
            : base(options, logger, encoder, clock)
        {
            _jwtHandler = jwtHandler;
            _userManager = userManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string errorReason;
            if (!Context.Request.Headers.TryGetValue(Options.AuthorizationHeader, out StringValues headerValue))
            {
                errorReason = $"Missing or malformed '{Options.AuthorizationHeader}' header.";

                return await Task.FromResult(AuthenticateResult.Fail(errorReason));
            }

            var authorizationHeader = headerValue.First();
            if (!authorizationHeader.StartsWith(Options.SignInScheme + ' ', StringComparison.OrdinalIgnoreCase))
            {
                errorReason = "Malformed 'Authorization' header.";

                return await Task.FromResult(AuthenticateResult.Fail(errorReason));
            }

            try
            {
                string token = authorizationHeader.Substring(SignInScheme.Length).Trim();
                var jwtValidationResult = _jwtHandler.Validate(token);
                if (jwtValidationResult.Succeeded)
                {
                    var jwtPayLoad = jwtValidationResult.JwtPayLoad;
                     long.TryParse(jwtPayLoad.Id, out var userId);
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null )
                    {
                        errorReason = "User Not Found";

                        return await Task.FromResult(AuthenticateResult.Fail(errorReason));
                    }

                    if (!await _userManager.IsUserActiveAsync(user))
                    {
                        errorReason = "User Not Active";

                        return await Task.FromResult(AuthenticateResult.Fail(errorReason));
                    }

                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Authentication, Options.Authentication),
                        new Claim("Token", token),
                        new Claim("Sub", jwtPayLoad.Id),
                        new Claim(ClaimTypes.Name, jwtPayLoad.Id),
                        new Claim(ClaimTypes.NameIdentifier, user.Username)
                    };
                    var id = new ClaimsIdentity(claims, Options.SignInScheme);
                    var identity = new ClaimsIdentity(id);
                    Options.Identity = identity;
                    var result = Task.FromResult(
                        AuthenticateResult.Success(
                            new AuthenticationTicket(
                                new ClaimsPrincipal(Options.Identity),
                                new AuthenticationProperties(),
                                Scheme.Name)));
                    return await result;
                }

                errorReason = string.Join("; ", jwtValidationResult.Errors);
                return await Task.FromResult(AuthenticateResult.Fail(errorReason));
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return await Task.FromResult(AuthenticateResult.Fail(e));
            }
        }
        
        
        
        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var authResult = await HandleAuthenticateOnceSafeAsync();
            if (!authResult.Succeeded)
            {
                ErrorResponse error;
                var builder = new StringBuilder();
                var exception = authResult.Failure;
                if (exception is UnAuthorizedException agEx)
                {
                    error = new ErrorResponse()
                    {
                        Code = agEx.Code,
                        Message = agEx.Message
                    };
                }
                else
                {
                    while (exception != null)
                    {
                        builder.Append(exception.Message);
                        exception = exception.InnerException;
                    }
                    var fullMessage = builder.ToString();
                    
                    error = new ErrorResponse()
                    {
                        Code = "AUTHFAILED",
                        Message = fullMessage
                    };
                }
                DefaultContractResolver contractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                };
                var serializerSettings = new JsonSerializerSettings()
                {
                    ContractResolver = contractResolver,
                    Formatting = Formatting.Indented
                };
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                Response.ContentType = "application/json";
                var response = JsonConvert.SerializeObject(error, serializerSettings);
                await Response.WriteAsync(response);
            }
            else
            {
                await base.HandleChallengeAsync(properties);
            }

        }
    }
}