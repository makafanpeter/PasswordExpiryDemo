using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PasswordPoliciesDemo.API.Infrastructure.Application.Configurations;
using PasswordPoliciesDemo.API.Infrastructure.Auth.Token;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.Infrastructure.Domain;
using PasswordPoliciesDemo.API.Infrastructure.Persistence;

namespace PasswordPoliciesDemo.API.Infrastructure.Services.Identity
{
    public class UserManager : IUserManager
    {
        private readonly IJwtHandler  _jwtHandler;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
        private readonly DemoContext _context;
        private readonly ApplicationUserSettings  _applicationUserSettings;

        public UserManager(IJwtHandler jwtHandler,
            IPasswordHasher<ApplicationUser> passwordHasher,
            DemoContext demoContext, 
            IOptions<ApplicationUserSettings> applicationUserSettings)
        {
            _jwtHandler = jwtHandler;
            _passwordHasher = passwordHasher;
            _context = demoContext;
            _applicationUserSettings = applicationUserSettings.Value;
        }

        public async Task<JsonWebToken> CreateLoginAsync(ApplicationUser user, int? tokenExpireAt)
        {

            var jwt = _jwtHandler.Create(user.Id.ToString(), tokenExpireAt);
            var refreshToken = await HashPassword(user, Guid.NewGuid().ToString());

            refreshToken = refreshToken.Replace("+", string.Empty)
                .Replace("=", string.Empty)
                .Replace("/", string.Empty);
            jwt.RefreshToken = refreshToken;

            user.FailedLoginAttempts = 0;
            user.CannotLoginUntil = null;
            user.LastLogin = DateTimeOffset.UtcNow;
            user.RefreshToken = refreshToken;
            await UpdateAsync(user);
            return jwt;
        }

        private async Task<string> HashPassword(ApplicationUser user, string password)
        {
            return await Task.FromResult(_passwordHasher.HashPassword(user, password));
        }

        public async Task<SecurityResult> CreateAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));


            await _context.ApplicationUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            return SecurityResult.Success;
        }

        public async Task<SecurityResult> UpdateAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            _context.ApplicationUsers.Update(user);
            await _context.SaveChangesAsync();

            return SecurityResult.Success;
        }


        public async Task<ApplicationUser> FindByIdAsync(long userId)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<ApplicationUser> FindByIdAsync(string userName)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Username == userName);

        }

        public Task<bool> IsUserActiveAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.Active);
        }

        public async Task<bool> HasPasswordExpiredAsync(ApplicationUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (_applicationUserSettings.PasswordLifetimeDays == 0)
                return false;

            int currentLifetime = int.MaxValue;
            if (user.LastPasswordChangedDate.HasValue)
            {
                currentLifetime = (DateTimeOffset.UtcNow - user.LastPasswordChangedDate.Value).Days;

            }
            return await Task.FromResult(currentLifetime >= _applicationUserSettings.PasswordLifetimeDays);
        }

        public async Task<SecurityResult> AccessFailedAsync(long id)
        {
            var user = await FindByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), id);
            }

            // If this puts the user over the threshold for lockout, lock them out and reset the access failed count;
            user.FailedLoginAttempts++;
            if (_applicationUserSettings.FailedPasswordAllowedAttempts > 0 &&
                user.FailedLoginAttempts >= _applicationUserSettings.FailedPasswordAllowedAttempts)
            {
                //lock out
                user.CannotLoginUntil = DateTime.UtcNow.AddMinutes(_applicationUserSettings.FailedPasswordLockoutMinutes);
                //reset the counter
                user.FailedLoginAttempts = 0;
            }
            return await UpdateAsync(user);
        }

        public async Task<bool> IsLockedOutAsync(long id)
        {
            var user = await FindByIdAsync(id);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), id);
            }
            if (user.CannotLoginUntil == null)
            {
                return false;
            }
            var lockoutTime = user.CannotLoginUntil;
            return lockoutTime >= DateTimeOffset.UtcNow;
        }

        public async Task<bool> VerifyPasswordAsync(ApplicationUser user, string password)
        {
            if (string.IsNullOrEmpty(user.Password) || string.IsNullOrEmpty(password))
            {
                return await Task.FromResult(false);
            }
            return _passwordHasher.VerifyHashedPassword(user, user.Password, password) == PasswordVerificationResult.Success;
        }

        public async Task<SecurityResult> RemoveLoginAsync(long userId)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }
            user.RefreshToken = null;
            await UpdateAsync(user);
            return SecurityResult.Success;
        }

        public async Task<ApplicationUser> GetUserByTokenAsync(string token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.RefreshToken == token);
        }

        public async Task<SecurityResult> ChangePasswordAsync(long userId, string oldPassword, string newPassword)
        {
            var user = await FindByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }
            if (await VerifyPasswordAsync(user, oldPassword))
            {
                var result = await UpdatePassword(user, newPassword);
                if (!result.Succeeded)
                {
                    return result;
                }

                return await UpdateAsync(user);
            }
            return SecurityResult.Failed("Password Mismatch");
        }

        public SecurityResult ValidatePasswordRequirement(string password)
        {
            

            //1. checks the value
            if (string.IsNullOrEmpty(password))
            {
                return SecurityResult.Failed("The password cannot be empty");
            }

            //2. Validate minimum length
            if (password.Length < _applicationUserSettings.PasswordMinimumLength)
            {
                return SecurityResult.Failed($"The password must be over {_applicationUserSettings.PasswordMinimumLength} characters.");
            }

            //3. At least one lowercase character
            if (_applicationUserSettings.PasswordRequireLowercase)
            {
                Match lowercase = Regex.Match(password, @"^(?=.*[a-z])");
                if (!lowercase.Success)
                {
                    return SecurityResult.Failed("The password must contain at least one lowercase character.");
                }
            }

            //4.  At least one upper case character
            if (_applicationUserSettings.PasswordRequireUppercase)
            {
                Match uppercase = Regex.Match(password, @"^(?=.*[A-Z])");
                if (!uppercase.Success)
                {
                   return SecurityResult.Failed("The password must contain at least one uppercase character.");
                }
            }

            // 3. At least one digit
            if (_applicationUserSettings.PasswordRequireDigit)
            {
                Match digit = Regex.Match(password, @"^(?=.*\d)");
                if (!digit.Success)
                {
                   return SecurityResult.Failed("The password must contain at least one digit.");
                }
            }

            // 4. At least one special character
            if (_applicationUserSettings.PasswordRequireNonAlphanumeric)
            {
                Match specialCharacter = Regex.Match(password, @"^(?=.*[^\da-zA-Z])");
                if (!specialCharacter.Success)
                {
                    return SecurityResult.Failed("The password must contain at least one non-alphanumeric character.");
                }
            }

            return SecurityResult.Success;
        }

        public async Task<SecurityResult> UpdatePassword(ApplicationUser user, string password)
        {
            var result = ValidatePasswordRequirement(password);
            if (!result.Succeeded)
            {
                return result;
            }

            user.LastPasswordChangedDate = DateTimeOffset.UtcNow;
            user.Password = await HashPassword(user, password);

            return SecurityResult.Success;
        }

        public async Task<ApplicationUser> FindByNameAsync(string username)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(x => x.Username == username);
        }

    }
}