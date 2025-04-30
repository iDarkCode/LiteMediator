using LiteMediator.Abstractions;
using LiteMediator.Behaviors;
using LiteMediator.Exceptions;
using System.Collections.Concurrent;

namespace LiteMediator.Core;

public class Mediator(ServiceFactory serviceFactory) : IMediator
{
    private readonly ServiceFactory _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));

    private readonly ConcurrentDictionary<Type, object> _requestHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _notificationHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _streamHandlers = [];
    private readonly ConcurrentDictionary<Type, object> _pipelineBehaviors = [];

  public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
    
        var requestType = request.GetType();
    
        // Obtener el handler
        var handler = (IRequestHandler<IRequest<TResponse>, TResponse>)_requestHandlers.GetOrAdd(
            requestType,
            static (type, factory) =>
            {
                var handlerType = typeof(IRequestHandler<,>).MakeGenericType(type, typeof(TResponse));
                return factory(handlerType);
            },
            _serviceFactory);
    
        if (handler is null)
            throw new HandlerNotFoundException(requestType);
    
        // Obtener los behaviors
        var behaviors = (IEnumerable<IPipelineBehavior<IRequest<TResponse>, TResponse>>)_pipelineBehaviors.GetOrAdd(
            requestType,
            static (type, factory) =>
            {
                var behaviorType = typeof(IEnumerable<>).MakeGenericType(typeof(IPipelineBehavior<,>).MakeGenericType(type, typeof(TResponse)));
                return factory(behaviorType) ?? Enumerable.Empty<IPipelineBehavior<IRequest<TResponse>, TResponse>>();
            },
            _serviceFactory);
    
        return InvokePipeline((IRequest<TResponse>)request, cancellationToken, behaviors, handler);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
    {
        if (notification is null) throw new ArgumentNullException(nameof(notification));

        var handlers = (IEnumerable<INotificationHandler<TNotification>>)_notificationHandlers.GetOrAdd(
            typeof(TNotification),
            static (notificationType, factory) =>
            {
                var handlerType = typeof(IEnumerable<>).MakeGenericType(typeof(INotificationHandler<>).MakeGenericType(notificationType));
                return factory(handlerType) ?? Array.Empty<INotificationHandler<TNotification>>();
            },
            _serviceFactory
        );

        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public async Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        if (notification is null) throw new ArgumentNullException(nameof(notification));

        var notificationType = notification.GetType();

        var handlerType = typeof(IEnumerable<>).MakeGenericType(typeof(INotificationHandler<>).MakeGenericType(notificationType));
        var handlers = (IEnumerable<object>)_serviceFactory(handlerType) ?? Array.Empty<object>();

        var tasks = handlers.Select(handler =>
        {
            var method = handler.GetType().GetMethod(nameof(INotificationHandler<INotification>.Handle))!;
            return (Task)method.Invoke(handler, new object[] { notification, cancellationToken })!;
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var handler = _streamHandlers.GetOrAdd(
            request.GetType(),
            static (requestType, factory) =>
            {
                var handlerType = typeof(IStreamRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
                var resolved = factory(handlerType);
                return resolved ?? throw new HandlerNotFoundException(requestType);
            },
            _serviceFactory
        );

       return ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }

    private Task<TResponse> InvokePipeline<TRequest, TResponse>(TRequest request,
                CancellationToken cancellationToken,
                IEnumerable<IPipelineBehavior<TRequest, TResponse>> behaviors,
                IRequestHandler<TRequest, TResponse> handler) where TRequest : IRequest<TResponse>
    {
        return PipelineBehaviorExecutor.ExecutePipeline(
            request,
            cancellationToken,
            behaviors,
            () => handler.Handle(request, cancellationToken));
    }

}

public delegate object? ServiceFactory(Type serviceType);


