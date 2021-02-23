using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.Infrastructure.Domain;
using PasswordPoliciesDemo.API.Infrastructure.Filters;
using PasswordPoliciesDemo.API.Infrastructure.Services.Identity;
using PasswordPoliciesDemo.API.ViewModels;

namespace PasswordPoliciesDemo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ValidatePassword]
    public class IdentityController : ControllerBase
    {

        private readonly IUserManager _userManager;
        private readonly ILogger<IdentityController> _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public IdentityController(IUserManager userManager,

            ILogger<IdentityController> logger, IMapper mapper, 
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _logger = logger;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }



        [HttpPost, Route("login")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            var user = await _userManager.FindByIdAsync(request.Username);

            if (user == null)
            {
                _logger.LogInformation($"User #{request.Username} Not Found");
                throw new NotFoundException("Either username or credential provided is invalid");
            }


            if (!await _userManager.IsUserActiveAsync(user))
            {
                _logger.LogInformation($"User #{request.Username} Not Active");
                throw new UnAuthorizedException("User account is not active");
            }

            var lockedOut = await _userManager.IsLockedOutAsync(user.Id);
            if (lockedOut)
                throw new UnAuthorizedException("User is locked out contact administrator");


            var validPassword = await _userManager.VerifyPasswordAsync(user, request.Password);
            if (!validPassword)
            {
                _logger.LogInformation($"User #{request.Username} Entered a wrong password");
                await _userManager.AccessFailedAsync(user.Id);

                throw new UnAuthorizedException("Either username or credential provided is invalid");
            }

            var response = new LoginResponse
            {
                Status = await _userManager.HasPasswordExpiredAsync(user)
                    ? AuthenticationStatus.RequirePasswordChange.ToString()
                    : AuthenticationStatus.Succeed.ToString(),
                JsonWebToken = await _userManager.CreateLoginAsync(user, request.TokenExpireAt),
                UserDetails = _mapper.Map<UserProfile>(user)
            };


            return Ok(response);
        }




        [HttpPost, Route("logout"), Authorize]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Logout()
        {
            var currentUser = _currentUserService.CurrentUser;

            if (currentUser == null)
            {

                throw new NotFoundException("User token was not found.");
            }

            //Revoke Refresh Token
            var result = await _userManager.RemoveLoginAsync(currentUser.Id);
            
            if (!result.Succeeded)
            {
                throw new BadRequestException(string.Join(",", result.Errors));
            }
            return NoContent();
        }

        [HttpPost, Route("refreshToken")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var user = await _userManager.GetUserByTokenAsync(request.Token);

            if (user == null)
            {
                _logger.LogInformation($"User #{request.Token} Not Found");
                throw new NotFoundException("User associated with the token not found");
            }


            var response = new LoginResponse
            {
                UserDetails = _mapper.Map<UserProfile>(user),
                Status = await _userManager.HasPasswordExpiredAsync(user) ? AuthenticationStatus.RequirePasswordChange.ToString() : AuthenticationStatus.Succeed.ToString(),
                JsonWebToken = await _userManager.CreateLoginAsync(user, null)
            };

            return Ok(response);
        }



        [HttpPost, Route("changePassword")]
        [Authorize]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (_currentUserService.CurrentUser == null)
            {
                throw new NotFoundException("The specified user could not be found");
            }
            var user = _currentUserService.CurrentUser;
            if (!await _userManager.VerifyPasswordAsync(user, request.OldPassword))
            {
                throw new BadRequestException("Old password doesn't match");
            }


            var result = await _userManager.ChangePasswordAsync(user.Id, request.OldPassword, request.NewPassword);

            if (!result.Succeeded)
            {
                throw new BadRequestException(string.Join(",", result.Errors));
            }
            return NoContent();
        }



        [HttpGet, Route("profile")]
        [Authorize]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UserProfile), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(_currentUserService.CurrentUser.Id);
            var userDetails = _mapper.Map<UserProfile>(user);
            return Ok(userDetails);
        }



        [HttpPost, Route("register")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
           

          



            if (string.IsNullOrEmpty(request.Username))
            {
                throw new BadRequestException("Username is required.");
            }

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user != null)
            {
                throw new BadRequestException("The specified username already exists");
            }

            user = new ApplicationUser()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Username = request.Username,
                Active =  true,
            };


            var result = await _userManager.UpdatePassword(user, request.Password);
            if (!result.Succeeded)
            {
                throw new BadRequestException(string.Join(",", result.Errors));
            }

            await _userManager.CreateAsync(user);

            var userDetails = _mapper.Map<UserProfile>(user);


            return Ok(userDetails);
        }


    }

    public enum AuthenticationStatus
    {
        Succeed,
        Require2Fa,
        RequirePasswordChange,
        Failed
    }
}
