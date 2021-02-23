using System.Collections.Generic;
using System.Linq;

namespace PasswordPoliciesDemo.API.Infrastructure.Auth.Token
{
    public class JwtValidationResult
    {
        private static readonly JwtValidationResult _success = new JwtValidationResult { Succeeded = true };
        private readonly List<string> _errors = new List<string>();

        /// <summary>
        /// Flag indicating whether if the operation succeeded or not.
        /// </summary>
        /// <value>True if the operation succeeded, otherwise false.</value>
        public bool Succeeded { get; protected set; }

        /// <summary>
        /// An <see cref="IEnumerable{T}"/> of <see cref="JwtValidationResult"/>s containing an errors
        /// that occurred during the identity operation.
        /// </summary>
        /// <value>An <see cref="IEnumerable{T}"/> of <see cref="JwtValidationResult"/>s.</value>
        public IEnumerable<string> Errors => _errors;

        /// <summary>
        /// Returns an <see cref="JwtValidationResult"/> indicating a successful identity operation.
        /// </summary>
        /// <returns>An <see cref="JwtValidationResult"/> indicating a successful operation.</returns>
        public static JwtValidationResult Success => _success;

        /// <summary>
        /// Creates an <see cref="JwtValidationResult"/> indicating a failed identity operation, with a list of <paramref name="errors"/> if applicable.
        /// </summary>
        /// <param name="errors">An optional array of <see cref="JwtValidationResult"/>s which caused the operation to fail.</param>
        /// <returns>An <see cref="JwtValidationResult"/> indicating a failed identity operation, with a list of <paramref name="errors"/> if applicable.</returns>
        public static JwtValidationResult Failed(params string[] errors)
        {
            var result = new JwtValidationResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.AddRange(errors);
            }
            return result;
        }


        public static JwtValidationResult Failed(string errors)
        {
            var result = new JwtValidationResult { Succeeded = false };
            if (errors != null)
            {
                result._errors.Add(errors);
            }
            return result;
        }

        /// <summary>
        /// Converts the value of the current <see cref="JwtValidationResult"/> object to its equivalent string representation.
        /// </summary>
        /// <returns>A string representation of the current <see cref="JwtValidationResult"/> object.</returns>
        /// <remarks>
        /// If the operation was successful the ToString() will return "Succeeded" otherwise it returned 
        /// "Failed : " followed by a comma delimited list of error codes from its <see cref="Errors"/> collection, if any.
        /// </remarks>
        public override string ToString()
        {
            return Succeeded ?
                   "Succeeded" : $"Failed : {string.Join(",", Errors.Select(x => x).ToList())}";
        }

        public JwtPayLoad JwtPayLoad { get; set; }
    }
}
