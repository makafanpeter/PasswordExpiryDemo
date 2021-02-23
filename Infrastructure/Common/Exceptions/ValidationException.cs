using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class ValidationException : DemoException
    {
        public ValidationException()
            : base("One or more validation failures have occurred.", "Invalid Request")
        {
            Failures = new Dictionary<string, string[]>();
        }

        public ValidationException(List<ValidationFailure> failures)
            : this()
        {
            //var message = string.Empty;
            var failureGroups = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage);

            foreach (var failureGroup in failureGroups)
            {
                var propertyName = failureGroup.Key;
                var propertyFailures = failureGroup.ToArray();

                Failures.Add(propertyName, propertyFailures);
                //message = string.Join(";", propertyFailures);
            }

        }

        public IDictionary<string, string[]> Failures { get; set; }
    }
}