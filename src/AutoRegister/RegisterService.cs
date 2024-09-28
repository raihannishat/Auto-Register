namespace AutoRegister;

internal static class RegisterService
{
    internal static void AddServices(IServiceCollection services, Assembly assembly)
    {
        List<Type>? typesWithInjectableAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<RegisterAttribute>() != null)
            .ToList();

        foreach (var type in typesWithInjectableAttribute)
        {
            var attribute = type.GetCustomAttribute<RegisterAttribute>();
            var rootBaseInfo = FindTopBaseType(type);

            var interfaces = rootBaseInfo.interfaces;
            var baseType = rootBaseInfo.topBasedType;

            // Self-registration only if no interfaces or abstract base classes
            if (interfaces.Length == 0 && baseType == null)
            {
                RegisterServiceIfNotRegistered(services, attribute!.Lifetime, type, type);
            }

            // AddServices the abstract base class if applicable
            if (baseType != null && baseType != typeof(object) && baseType.IsAbstract)
            {
                RegisterServiceIfNotRegistered(services, attribute!.Lifetime, baseType, type);
                RegisterServiceIfNotRegistered(services, attribute!.Lifetime, type, type);
            }

            // AddServices all implemented interfaces
            if (interfaces.Length > 0)
            {
                foreach (var @interface in interfaces)
                {
                    RegisterServiceIfNotRegistered(services, attribute!.Lifetime, @interface, type);
                    RegisterServiceIfNotRegistered(services, attribute!.Lifetime, type, type);
                }
            }
        }
    }

    private static void RegisterServiceIfNotRegistered(
        IServiceCollection services,
        ServiceLifetime lifetime,
        Type serviceType,
        Type implementationType)
    {
        if (!services.Any(sd => sd.ServiceType == serviceType && sd.ImplementationType == implementationType))
        {
            _ = lifetime switch
            {
                ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
                ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
                ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
                _ => throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null)
            };
        }
    }

    private static (Type? topBasedType, Type[] interfaces) FindTopBaseType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type topBaseType = type;

        // Traverse up to find the topmost base type
        while (topBaseType.BaseType != null && topBaseType.BaseType != typeof(object))
        {
            topBaseType = topBaseType.BaseType;
        }

        var interfaces = type.GetInterfaces();

        return topBaseType == type ? (null, interfaces) : (topBaseType, interfaces);
    }
}
