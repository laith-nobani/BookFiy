using System;
using System.Collections.Generic;
using System.Text;

namespace BookFiy.Application.Comman
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public ErrorType? ErrorType { get; set; }
        public Result(bool isSuccess, string message, T? data, ErrorType? errorType)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
            ErrorType = errorType;
        }
        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T>(true, message, data,null);
        }
        public static Result<T> Failure(string message,ErrorType errorType)
        {
            return new Result<T>(false, message, default,errorType);
        }

    }
}
