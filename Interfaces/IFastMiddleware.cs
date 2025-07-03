using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace FastMiddleware.Interfaces
{
    public interface IFastMiddleware
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

        Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;
    }
}