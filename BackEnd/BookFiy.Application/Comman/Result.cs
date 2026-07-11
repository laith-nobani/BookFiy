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
        public Result(bool isSuccess, string message, T? data)
        {
            IsSuccess = isSuccess;
            Message = message;
            Data = data;
        }
        public static Result<T> Success(T data, string message = "")
        {
            return new Result<T>(true, message, data);
        }
        public static Result<T> Failure(string message)
        {
            return new Result<T>(false, message, default);
        }

    }
}
