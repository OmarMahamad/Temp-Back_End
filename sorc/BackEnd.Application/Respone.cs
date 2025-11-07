using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Application
{
    public class Response
    {
        public bool IsSuccess { get; protected set; }
        public string Message { get; protected set; } = string.Empty;
        public StatusCodeResponeType Code { get; protected set; }
        public IReadOnlyDictionary<string, string[]>? Errors { get; protected set; }

        protected Response() { }

        // Success
        public static Response Success(string message = "Operation completed successfully",
                                       StatusCodeResponeType code = StatusCodeResponeType.Success)
            => new Response { IsSuccess = true, Message = message, Code = code };

        // Failure (بدون تفاصيل)
        public static Response Failure(string message,
                                       StatusCodeResponeType code = StatusCodeResponeType.InternalServerError)
            => new Response { IsSuccess = false, Message = message, Code = code };

        // Failure (مع تفاصيل الأخطاء)
        public static Response Failure(string message,
                                       StatusCodeResponeType code,
                                       IDictionary<string, string[]> errors)
            => new Response
            {
                IsSuccess = false,
                Message = message,
                Code = code,
                Errors = new Dictionary<string, string[]>(errors)
            };
    }

    public class Response<T> : Response
    {
        public T? Data { get; private set; }

        private Response() { }

        // Success
        public static Response<T> Success(T data, string message = "Success",
                                          StatusCodeResponeType code = StatusCodeResponeType.Success)
            => new Response<T> { IsSuccess = true, Message = message, Code = code, Data = data };

        // Failure (بدون تفاصيل)
        public static Response<T> Failure(string message,
                                          StatusCodeResponeType code = StatusCodeResponeType.InternalServerError)
            => new Response<T> { IsSuccess = false, Message = message, Code = code };

        // Failure (مع تفاصيل الأخطاء)
        public static Response<T> Failure(string message,
                                          StatusCodeResponeType code,
                                          IDictionary<string, string[]> errors)
            => new Response<T>
            {
                IsSuccess = false,
                Message = message,
                Code = code,
                Errors = new Dictionary<string, string[]>(errors)
            };
    }

}
