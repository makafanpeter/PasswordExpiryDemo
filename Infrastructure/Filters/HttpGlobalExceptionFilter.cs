using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.Infrastructure.Common.Extensions;
using PasswordPoliciesDemo.API.ViewModels;

namespace PasswordPoliciesDemo.API.Infrastructure.Filters
{
    public class HttpGlobalExceptionFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HttpGlobalExceptionFilter> _logger;

        public HttpGlobalExceptionFilter(IWebHostEnvironment env, ILogger<HttpGlobalExceptionFilter> logger)
        {
            _env = env;
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogInformation($"Environment : {_env.EnvironmentName}");
            _logger.LogError(new EventId(context.Exception.HResult),
                context.Exception,
                context.Exception.Message);
            HttpStatusCode code;
            ErrorResponse response;
            switch (context.Exception)
            {
                case NotFoundException e:
                    code = HttpStatusCode.NotFound;
                    response = e.ToErrorResponse();
                    break;
                case ValidationException e:
                    code = HttpStatusCode.BadRequest;
                    response = e.ToErrorResponse();
                    break;
                case UnAuthorizedException e:
                    code = HttpStatusCode.Unauthorized;
                    response = e.ToErrorResponse();
                    break;
                case ForbiddenException e:
                    code = HttpStatusCode.Forbidden;
                    response = e.ToErrorResponse();
                    break;
                case BadRequestException e:
                    code = HttpStatusCode.BadRequest;
                    response = e.ToErrorResponse();
                    break;
                case ServiceUnavailableException e:
                    code = HttpStatusCode.ServiceUnavailable;
                    response = e.ToErrorResponse();
                    break;
                case SystemErrorException e:
                    code = HttpStatusCode.InternalServerError;
                    response = e.ToErrorResponse();
                    break;
                case ConflictException e:
                    code = HttpStatusCode.Conflict;
                    response = e.ToErrorResponse();
                    break;
                default:
                    code = HttpStatusCode.InternalServerError;
                    response = context.Exception.ToErrorResponse();
                    break;
            }

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
            var serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            };
            var result = JsonConvert.SerializeObject(response, serializerSettings);
            context.HttpContext.Response.ContentType = "application/json";
            context.HttpContext.Response.StatusCode = (int)code;
            context.HttpContext.Response.WriteAsync(result);
            context.ExceptionHandled = true;
        }



    }
}
