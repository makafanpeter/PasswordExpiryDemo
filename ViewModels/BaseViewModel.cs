using System;

namespace PasswordPoliciesDemo.API.ViewModels
{
    public abstract class BaseViewModel
    {
        public long Id { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset? UpdatedOn { get; set; }


    }
}