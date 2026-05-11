using BlueBerryFinance.API.Domain.Entities;

namespace BlueBerryFinance.API.Infrastructure.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
