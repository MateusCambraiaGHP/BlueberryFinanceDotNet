namespace BlueBerryFinance.API.Application.Features.Common
{
    public class BaseRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
