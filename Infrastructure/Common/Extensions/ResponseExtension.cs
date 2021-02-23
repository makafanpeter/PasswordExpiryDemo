using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions;
using PasswordPoliciesDemo.API.ViewModels;

namespace PasswordPoliciesDemo.API.Infrastructure.Common.Extensions
{
    public static class ResponseExtension
    {

        public static ErrorResponse ToErrorResponse(this Exception e)
        {
            return new ErrorResponse()
            {
                Code = "SYSTEM_ERROR",
                Message = "Unexpected error occured please try again or confirm current operation status"
            };
        }


        public static ErrorResponse ToErrorResponse(this NotFoundException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }

        public static ErrorResponse ToErrorResponse(this ConflictException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }


        public static ErrorResponse ToErrorResponse(this BadRequestException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }


        public static ErrorResponse ToErrorResponse(this ValidationException e)
        {
            var msg = string.Empty;
            foreach (var item in e.Failures)
            {
                var errors = item.Value.ToList();
                msg += errors.Aggregate("",
                    (current, next) => $"{current} {next}");


            }

            return new ValidationErrorResponse()
            {
                Code = e.Code,
                Message = $"{e.Message} {msg}",
                Errors = e.Failures
            };
        }


        public static ErrorResponse ToErrorResponse(this ServiceUnavailableException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }

        public static ErrorResponse ToErrorResponse(this SystemErrorException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }


        public static ErrorResponse ToErrorResponse(this UnAuthorizedException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }


        public static ErrorResponse ToErrorResponse(this ForbiddenException e)
        {
            return new ErrorResponse()
            {
                Code = e.Code,
                Message = e.Message
            };
        }



        public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            using (StreamReader reader = new StreamReader(request.Body, encoding))
                return await reader.ReadToEndAsync();
        }
    }
}
