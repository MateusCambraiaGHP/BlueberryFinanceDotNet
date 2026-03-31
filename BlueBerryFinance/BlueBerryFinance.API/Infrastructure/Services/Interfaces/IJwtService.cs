using BlueBerryFinance.API.Data.Entities;

namespace BlueBerryFinance.API.Infrastructure.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
