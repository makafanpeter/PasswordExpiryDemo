using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PasswordPoliciesDemo.API.Infrastructure.Application.Configurations;

namespace PasswordPoliciesDemo.API.Infrastructure.Auth.Token
{
    public class JwtHandler : IJwtHandler
    {
        private readonly JwtSettings _options;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        private readonly JwtHeader _jwtHeader;
        private readonly ILogger _logger;

        public JwtHandler(IOptions<JwtSettings> options, ILogger<JwtHandler> logger)
        {
            _logger = logger;
            _options = options.Value;
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            _jwtHeader = new JwtHeader(signingCredentials);
        }

        public JsonWebToken Create(string username, int? expiryMinutes = 0)
        {
            var nowUtc = DateTimeOffset.UtcNow;

            var expires = expiryMinutes > 0 ? nowUtc.AddMinutes(expiryMinutes.Value) : nowUtc.AddMinutes(_options.ExpiryMinutes);


            var centuryBegin = new DateTime(1970, 1, 1).ToUniversalTime();
            var exp = (long)(new TimeSpan(expires.Ticks - centuryBegin.Ticks).TotalSeconds);
            var iat = (long)(new TimeSpan(nowUtc.Ticks - centuryBegin.Ticks).TotalSeconds);
            var payload = new JwtPayload
            {
                {"sub", username},
                {"iss", _options.Issuer},
                {"iat", iat},
                {"exp", exp},
                {"unique_name", username},
                {"aud", _options.Issuer}
            };
            var jwt = new JwtSecurityToken(_jwtHeader, payload);
            var token = _jwtSecurityTokenHandler.WriteToken(jwt);

            return new JsonWebToken
            {
                AccessToken = token,
                Expires = exp
            };
        }

        public JwtValidationResult Validate(string token)
        {
            var jwtValidationResult = JwtValidationResult.Success;
            try
            {
                if (_jwtSecurityTokenHandler.CanReadToken(token))
                {

                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = _options.ValidateIssuerSigningKey,
                        ValidIssuer = _options.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                        ClockSkew = TimeSpan.Zero,
                        ValidateAudience = _options.ValidateAudience,
                        ValidateLifetime = _options.ValidateLifetime,

                    };
                    var claims = _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                    jwtValidationResult.JwtPayLoad = new JwtPayLoad()
                    {
                        ValidTo = validatedToken.ValidTo,
                        Id = claims.Identity.Name

                    };
                }
                else
                {
                    jwtValidationResult = JwtValidationResult.Failed("Token Verification Failed");
                }
            }
            catch (ArgumentException e)
            {
                _logger.LogError(new EventId(203, GetType().Name), e, e.Message);
                jwtValidationResult = JwtValidationResult.Failed("Unable to decode token");
            }
            catch (Exception e)
            {
                _logger.LogError(new EventId(203, GetType().Name), e, e.Message);

                jwtValidationResult = JwtValidationResult.Failed(CreateErrorDescription(e).ToArray());
            }
            return jwtValidationResult;
        }


        private static List<string> CreateErrorDescription(Exception authFailure)
        {
            IEnumerable<Exception> exceptions;
            if (authFailure is AggregateException agEx)
            {
                exceptions = agEx.InnerExceptions;
            }
            else
            {
                exceptions = new[] { authFailure };
            }

            var messages = new List<string>();

            foreach (var ex in exceptions)
            {
                // Order sensitive, some of these exceptions derive from others
                // and we want to display the most specific message possible.
                if (ex is SecurityTokenInvalidAudienceException)
                {
                    messages.Add("The audience is invalid");
                }
                else if (ex is SecurityTokenInvalidIssuerException)
                {
                    messages.Add("The issuer is invalid");
                }
                else if (ex is SecurityTokenNoExpirationException)
                {
                    messages.Add("The token has no expiration");
                }
                else if (ex is SecurityTokenInvalidLifetimeException)
                {
                    messages.Add("The token lifetime is invalid");
                }
                else if (ex is SecurityTokenNotYetValidException)
                {
                    messages.Add("The token is not valid yet");
                }
                else if (ex is SecurityTokenExpiredException)
                {
                    messages.Add("The token is expired");
                }
                else if (ex is SecurityTokenSignatureKeyNotFoundException)
                {
                    messages.Add("The signature key was not found");
                }
                else if (ex is SecurityTokenInvalidSignatureException)
                {
                    messages.Add("The signature is invalid");
                }
            }

            return  messages;
        }
    }
}
