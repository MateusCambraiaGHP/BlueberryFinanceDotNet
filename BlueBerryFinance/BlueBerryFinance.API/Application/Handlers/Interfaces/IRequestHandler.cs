namespace BlueBerryFinance.API.Application.Handlers.Interfaces
{
    public interface IRequestHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default);
    }
}
