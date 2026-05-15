using System.Collections.Concurrent;
using System.Reflection;

namespace SchoolErp.Application.Common.Mediator;

/// <summary>
/// Marker interface for a request with a response.
/// Drop-in replacement for MediatR.IRequest&lt;TResponse&gt;.
/// </summary>
public interface IRequest<TResponse> { }

/// <summary>
/// Defines a handler for a request.
/// Drop-in replacement for MediatR.IRequestHandler&lt;TRequest, TResponse&gt;.
/// </summary>
public interface IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Pipeline behavior to surround the inner handler.
/// </summary>
public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a delegate for the next action in the pipeline.
/// </summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// Mediator contract for sending requests to their handlers.
/// </summary>
public interface IMediator
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Optimized mediator implementation using .NET's DI and cached delegates.
/// Supports pipeline behaviors (validation, logging, etc.).
/// </summary>
public class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> _handlerCache = new();

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = _serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException(
                $"No handler registered for '{requestType.Name}'. " +
                $"Ensure a class implementing IRequestHandler<{requestType.Name}, {typeof(TResponse).Name}> exists.");

        // Resolve behaviors
        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = (IEnumerable<object>)_serviceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(behaviorType))!;

        // Handler invocation delegate
        RequestHandlerDelegate<TResponse> handlerDelegate = async () => 
        {
            var wrapper = (HandlerWrapper<TResponse>)_handlerCache.GetOrAdd(requestType, t => 
            {
                var method = handlerType.GetMethod("Handle")!;
                return new HandlerWrapper<TResponse>(method);
            });
            return await wrapper.Invoke(handler, request, cancellationToken);
        };

        // Wrap handler in behaviors
        foreach (var behavior in behaviors.Reverse())
        {
            var next = handlerDelegate;
            var behaviorHandleMethod = behavior.GetType().GetMethod("Handle")!;
            handlerDelegate = () => (Task<TResponse>)behaviorHandleMethod.Invoke(behavior, new object[] { request, next, cancellationToken })!;
        }

        return await handlerDelegate();
    }

    private class HandlerWrapper<TResponse>
    {
        private readonly MethodInfo _method;
        public HandlerWrapper(MethodInfo method) => _method = method;
        public Task<TResponse> Invoke(object handler, object request, CancellationToken ct) 
            => (Task<TResponse>)_method.Invoke(handler, new[] { request, ct })!;
    }
}
