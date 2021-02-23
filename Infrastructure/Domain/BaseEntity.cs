using System;

namespace PasswordPoliciesDemo.API.Infrastructure.Domain
{
    public class BaseEntity
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }
    }
}