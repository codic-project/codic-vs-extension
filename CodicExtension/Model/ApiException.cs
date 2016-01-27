
using System;

namespace CodicExtension.Model
{
    public class ApiException : Exception
    {
        private int Code { get; }

        public ApiException(int code, String message) : base(message)
        {
            this.Code = code;
        }
    }
}