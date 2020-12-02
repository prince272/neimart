using System;
using System.Collections.Generic;
using System.Text;

namespace Neimart.Core
{
    public class Result
    {
        public bool Success { get; }

        public string Message { get; }

        protected Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static Result Fail(string message)
        {
            return new Result(false, message);
        }

        public static Result Ok(string message = null)
        {
            return new Result(true, message);
        }

        public static Result<TValue> Ok<TValue>(TValue value, string message = null)
        {
            return new Result<TValue>(value, true, message);
        }

        public static Result<TValue> Fail<TValue>(string message)
        {
            return new Result<TValue>(default(TValue), false, message);
        }
    }

    public class Result<TValue> : Result
    {
        public TValue Value { get; set; }

        protected internal Result(TValue value, bool success, string message)
            : base(success, message)
        {
            Value = value;
        }
    }
}
