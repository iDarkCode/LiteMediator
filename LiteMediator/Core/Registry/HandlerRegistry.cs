using LiteMediator.Behaviors;
using System.Collections.Concurrent;
using System.Reflection;

namespace LiteMediator.Core.Registry;

public class HandlerRegistry
{
    private readonly ConcurrentDictionary<Type, object> _requestHandlers = new();
    private readonly ConcurrentDictionary<Type, List<object>> _behaviors = new();
    private readonly List<Type> _openBehaviors = new();
    private readonly ServiceFactory _serviceFactory;

    public HandlerRegistry(ServiceFactory serviceFactory)
    {
        _serviceFactory = serviceFactory ?? throw new ArgumentNullException(nameof(serviceFactory));
    }

    public void RegisterRequestHandler(Type requestType, object handler)
    {
        _requestHandlers[requestType] = handler;
    }

    public void RegisterOpenBehavior(Type openBehaviorType)
    {
        if (!openBehaviorType.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic behavior types can be registered");

        _openBehaviors.Add(openBehaviorType);
    }

    public void RegisterBehaviorsFromAssembly(Assembly assembly)
    {
        var behaviorTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t =>
                t.GetInterfaces()
                 .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>))
                 .Select(i => (interfaceType: i, implementationType: t)))
            .ToList();

        foreach (var (iface, impl) in behaviorTypes)
        {
            var requestType = iface.GetGenericArguments()[0];
            var responseType = iface.GetGenericArguments()[1];
            var key = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, responseType);

            if (!_behaviors.TryGetValue(key, out var list))
            {
                list = new List<object>();
                _behaviors[key] = list;
            }

            list.Add(_serviceFactory(impl)!);
        }
    }

    public object? GetHandler(Type requestType)
        => _requestHandlers.TryGetValue(requestType, out var handler) ? handler : null;

    public IEnumerable<IPipelineBehavior<TRequest, TResponse>> GetBehaviors<TRequest, TResponse>()
    {
        var key = typeof(IPipelineBehavior<TRequest, TResponse>);
        var found = _behaviors.TryGetValue(key, out var list)
            ? list.Cast<IPipelineBehavior<TRequest, TResponse>>()
            : Enumerable.Empty<IPipelineBehavior<TRequest, TResponse>>();

        var fromOpen = _openBehaviors
            .Select(open => open.MakeGenericType(typeof(TRequest), typeof(TResponse)))
            .Select(b => _serviceFactory(b))
            .Where(b => b is not null)
            .Cast<IPipelineBehavior<TRequest, TResponse>>();

        return found.Concat(fromOpen);
    }
}
