using BlueBerryFinance.API.Application.Features.Common;
using BlueBerryFinance.Common.ViewModels;

namespace BlueBerryFinance.API.Application.Features.Currencies
{
    public interface ICurrencyHandler
    {
        Task<BaseResponse<CurrencyViewModel>> GetAsync();
    }
}
