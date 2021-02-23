using System.Threading.Tasks;
using PasswordPoliciesDemo.API.Infrastructure.Auth.Token;
using PasswordPoliciesDemo.API.Infrastructure.Domain;

namespace PasswordPoliciesDemo.API.Infrastructure.Services.Identity
{
    public interface IUserManager
    {
        /// <summary>
        ///     Creates a login representing the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="tokenExpireAt"></param>
        /// <returns></returns>
        Task<JsonWebToken> CreateLoginAsync(ApplicationUser user, int? tokenExpireAt);

        /// <summary>
        ///     Create a user with no password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<SecurityResult> CreateAsync(ApplicationUser user);

        /// <summary>
        ///     Update a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<SecurityResult> UpdateAsync(ApplicationUser user);


        /// <summary>
        ///     Find a user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<ApplicationUser> FindByIdAsync(long userId);

        /// <summary>
        ///     Find a user by name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<ApplicationUser> FindByIdAsync(string userName);
        /// <summary>
        /// If user is active
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> IsUserActiveAsync(ApplicationUser user);
        /// <summary>
        /// If User's password has expired
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> HasPasswordExpiredAsync(ApplicationUser user);
        /// <summary>
        /// Update users login attempt failure
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<SecurityResult> AccessFailedAsync(long id);
        /// <summary>
        ///  if user is locked out
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<bool> IsLockedOutAsync(long id);
        /// <summary>
        /// Verify Users password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<bool> VerifyPasswordAsync(ApplicationUser user, string password);
        /// <summary>
        /// Revoke token
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<SecurityResult> RemoveLoginAsync(long userId);
        /// <summary>
        /// Get Token for users
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetUserByTokenAsync(string token);
        /// <summary>
        /// Change users password
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<SecurityResult> ChangePasswordAsync(long userId, string oldPassword, string newPassword);
        /// <summary>
        /// Validate if password meets users requirement
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        SecurityResult ValidatePasswordRequirement(string password);
        /// <summary>
        /// Update User password
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<SecurityResult> UpdatePassword(ApplicationUser user, string password);
        /// <summary>
        /// Find User by name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        Task<ApplicationUser> FindByNameAsync(string username);
    }
}
