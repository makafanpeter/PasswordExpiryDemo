using System.Collections.Generic;
using System.Linq;

namespace PasswordPoliciesDemo.API.Infrastructure.Services.Identity
{
    public class SecurityResult
    {
        private static readonly SecurityResult _success = new SecurityResult { Succeeded = true };
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="SecurityResult"/>s containing an errors
        /// that occurred during the identity operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="SecurityResult"/>s.</value>
        public IEnumerable<string> Errors => _errors;

        /// <summary>
        /// Returns an <see cref="SecurityResult"/> indicating a successful identity operation.
        /// </summary>
        /// <returns>An <see cref="SecurityResult"/> indicating a successful operation.</returns>
        public static SecurityResult Success => _success;

        /// <summary>
        /// Creates an <see cref="SecurityResult"/> indicating a failed identity operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="SecurityResult"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="SecurityResult"/> indicating a failed identity operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static SecurityResult Failed(params string[] errors)
        {
            var result = new SecurityResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }


        public static SecurityResult Failed(string errors)
        {
            var result = new SecurityResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.Add(errors);
            }
            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="SecurityResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="SecurityResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned 
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                "Succeeded" : $"Failed : {string.Join(",", Errors.Select(x => x).ToList())}";
        }
    }
}