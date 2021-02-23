using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace PasswordPoliciesDemo.API.Auth.Token
{
    public class TokenAuthenticationOptions : AuthenticationSchemeOptions
    {
        public ClaimsIdentity Identity { get; set; }


        public string AuthorizationHeader
        {
            get { return "Authorization"; }
        }

        public string Authentication
        {
            get { return "Bearer"; }
        }

        public string SignInScheme => "Bearer";
        public string ChannelHeader => "Channel";
    }
}