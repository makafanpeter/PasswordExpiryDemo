using System;

namespace PasswordPoliciesDemo.API.Infrastructure.Common.Exceptions
{
    public class DemoException : Exception
    {
        public string Code { get; set; }
        public DemoException()
        { }

        public DemoException(string message)
            : base(message)
        { }

        public DemoException(string message, string code)
            : base(message)
        {
            Code = code;
        }

        public DemoException(string message, Exception innerException)
            : base(message, innerException)
        { }


        public override string ToString()
        {
            return $"Error : \n\nCode:{Code}\n\nMessage:{Message}";
        }

        public DemoException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

    }
}
