using BlueBerryFinance.API.Application.Requests.Auth;
using BlueBerryFinance.API.Application.Responses.Auth;

namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface IAuthHandler : IRequestHandler<LoginRequest, LoginResponse?>
    {
    }
}
