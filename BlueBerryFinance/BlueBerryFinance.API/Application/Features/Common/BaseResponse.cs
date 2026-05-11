namespace BlueBerryFinance.API.Application.Features.Common
{
    public class BaseResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public List<string> ValidationErrors { get; private set; } = new List<string>();
        public List<T>? Data { get; private set; }

        private BaseResponse(List<T> data, string message = "")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        private BaseResponse(T data, string message = "")
        {
            Success = true;
            Message = message;
            Data = new List<T>();
            Data.Add(data);
        }

        private BaseResponse(bool success, string message = "", List<string>? validationErrors = null)
        {
            Success = success;
            Message = message;
            ValidationErrors = validationErrors ?? new List<string>();
            Data = null;
        }

        public static BaseResponse<T> Ok(List<T> data, string message = "")
            => new BaseResponse<T>(data, message);

        public static BaseResponse<T> Ok(T data, string message = "")
            => new BaseResponse<T>(data, message);

        public static BaseResponse<T> Ok(string message = "")
            => new BaseResponse<T>(true, message);

        public static BaseResponse<T> Fail(string message)
            => new BaseResponse<T>(false, message);

        public static BaseResponse<T> Fail(List<string> validationErrors)
            => new BaseResponse<T>(false, "One or more errors", validationErrors);
    }
}
