using System;

namespace PasswordPoliciesDemo.API.Infrastructure.Auth.Token
{

    public class JwtPayLoad
    {
        public DateTime ValidTo { get; set; }
        public string Id { get; set; }
    }
}
