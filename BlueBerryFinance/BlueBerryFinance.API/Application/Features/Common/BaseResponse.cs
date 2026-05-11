using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Common
{
    public class BaseResponse<T>
    {
        public bool Success { get; private set; }
        public string Message { get; private set; } = string.Empty;
        public List<string> ValidationErrors { get; private set; } = [];
        public List<T>? Data { get; private set; }
        public PaginationMeta? Pagination { get; private set; }

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
            Data = [data];
        }

        private BaseResponse(bool success, string message = "", List<string>? validationErrors = null)
        {
            Success = success;
            Message = message;
            ValidationErrors = validationErrors ?? [];
            Data = null;
        }

        private BaseResponse(List<T> data, PaginationMeta pagination, string message = "")
        {
            Success = true;
            Message = message;
            Data = data;
            Pagination = pagination;
        }

        public static BaseResponse<T> Ok(List<T> data, string message = "")
            => new(data, message);

        public static BaseResponse<T> Ok(T data, string message = "")
            => new(data, message);

        public static BaseResponse<T> Ok(string message = "")
            => new(true, message);

        public static BaseResponse<T> Ok(List<T> data, int totalCount, int page, int pageSize, string message = "")
            => new(data, new PaginationMeta(totalCount, page, pageSize), message);

        public static BaseResponse<T> Fail(string message)
            => new(false, message);

        public static BaseResponse<T> Fail(List<string> validationErrors)
            => new(false, "One or more errors", validationErrors);
    }
}
