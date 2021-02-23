using System;

namespace PasswordPoliciesDemo.API.Infrastructure.Domain
{
    public class ApplicationUser:BaseEntity
    {
       

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Password { get; set; }


        public DateTimeOffset? LastPasswordChangedDate { get; set; }

        public string Username { get; set; }

        public DateTimeOffset? LastLogin { get; set; }
        public int FailedLoginAttempts { get; set; }
        public DateTimeOffset? CannotLoginUntil { get; set; }

        public string RefreshToken { get; set; }
        public bool Active { get; set; }
    }
}
