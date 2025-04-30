using System.Reflection;
using LiteMediator.Abstractions;
using LiteMediator.Behaviors;
using LiteMediator.Core;
using Microsoft.Extensions.DependencyInjection;

namespace LiteMediator.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLiteMediator(this IServiceCollection services, Action<LiteMediatorOptions>? configure = null)
    {
        var options = new LiteMediatorOptions();
        configure?.Invoke(options);

        services.Add(new ServiceDescriptor(typeof(IMediator), typeof(Mediator), options.Lifetime));

        // Registramos los handlers encontrados en los assemblies
        foreach (var assembly in options.Assemblies)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;

                    var def = iface.GetGenericTypeDefinition();

                    if (def == typeof(IRequestHandler<,>) ||
                        def == typeof(INotificationHandler<>) ||
                        def == typeof(IStreamRequestHandler<,>) ||
                        def == typeof(IPipelineBehavior<,>))
                    {
                        services.AddScoped(iface, type);
                    }
                }
            }
        }

        // Registramos los behaviors
        foreach (var openBehavior in options.OpenBehaviors)
        {
            services.AddScoped(typeof(IPipelineBehavior<,>), openBehavior);
        }

        return services;
    }
}



public class LiteMediatorOptions
{
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
    public Assembly[] Assemblies { get; set; } = Array.Empty<Assembly>();

    internal List<Type> OpenBehaviors { get; } = new();

    public void AddOpenBehavior(Type openBehavior)
    {
        if (!openBehavior.IsGenericTypeDefinition)
            throw new ArgumentException("Only open generic types are supported", nameof(openBehavior));

        var genericArgs = openBehavior.GetGenericArguments();
        if (genericArgs.Length != 2)
            throw new ArgumentException("Behavior must have two generic arguments", nameof(openBehavior));

        OpenBehaviors.Add(openBehavior);
    }
}
