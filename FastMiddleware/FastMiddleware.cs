using System;
using System.Threading;
using System.Threading.Tasks;
using FastMiddleware.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FastMiddleware
{
    public class FastMiddleware : IFastMiddleware
    {
        private readonly IServiceProvider _serviceProvider;

        public FastMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlerType = typeof(INotificationHandler<>).MakeGenericType(notification.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            if (handlers == null)
            {
                throw new InvalidOperationException($"Handler for request type {notification.GetType()} not found.");
            }
            
            foreach (var handler in handlers)
            {
                await (Task)handlerType
                .GetMethod("Handle")
                ?.Invoke(handler, new object[] { notification, cancellationToken });
            }
        }

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = _serviceProvider.GetService(handlerType);

            if (handler == null)
            {
                throw new InvalidOperationException($"Handler for request type {request.GetType()} not found.");
            }

            return await
            (Task<TResponse>)handlerType
            .GetMethod("Handle")
            ?.Invoke(handler, new object[] { request, cancellationToken })
            ?? throw new InvalidOperationException($"Handler for request type {request.GetType()} does not have a valid Handle method.");
        }
    }
}