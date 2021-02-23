using System;
using Microsoft.AspNetCore.Authentication;

namespace PasswordPoliciesDemo.API.Auth.Token
{
    public static class TokenAuthenticationExtension
    {
        public static AuthenticationBuilder AddTokenAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<TokenAuthenticationOptions> configureOptions)
        {
            return builder.AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}