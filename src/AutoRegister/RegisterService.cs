namespace AutoRegister;

internal static class RegisterService
{
    internal static void AddServices(IServiceCollection services, Assembly assembly)
    {
        List<Type> typesWithInjectableAttribute = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<RegisterAttribute>() != null)
            .ToList();

        foreach (var type in typesWithInjectableAttribute)
        {
            var attribute = type.GetCustomAttribute<RegisterAttribute>();
            var (baseType, interfaces) = FindTopBaseType(type);

            RegisterSelf(baseType, type, attribute!.Lifetime, services);
            RegisterAbstractBase(baseType, type, attribute.Lifetime, services);
            RegisterInterfaces(interfaces, type, attribute.Lifetime, services);
        }
    }

    private static void RegisterSelf(Type? baseType, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        if (!type.IsAbstract && type.GetInterfaces().Length == 0 && baseType == null)
        {
            if (type.IsGenericType && type.IsGenericTypeDefinition)
            {
                RegisterServiceIfNotRegistered(services, lifetime, type.GetGenericTypeDefinition(), type);
            }
            else
            {
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
        }
    }

    private static void RegisterAbstractBase(Type? baseType, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        if (baseType != null && baseType.IsAbstract && baseType != typeof(object))
        {
            if (baseType.IsGenericType && baseType.IsGenericTypeDefinition)
            {
                RegisterServiceIfNotRegistered(services, lifetime, baseType.GetGenericTypeDefinition(), type);
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
            else
            {
                RegisterServiceIfNotRegistered(services, lifetime, baseType, type);
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
        }
    }

    private static void RegisterInterfaces(Type[] interfaces, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        foreach (var @interface in interfaces)
        {
            if (@interface.IsGenericType)
            {
                RegisterGenericInterface(services, lifetime, @interface, type);
            }
            else
            {
                RegisterServiceIfNotRegistered(services, lifetime, @interface, type);
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
        }
    }

    private static void RegisterGenericInterface(IServiceCollection services, ServiceLifetime lifetime, Type @interface, Type implementation)
    {
        if (implementation.IsGenericType && implementation.IsGenericTypeDefinition)
        {
            RegisterServiceIfNotRegistered(services, lifetime, @interface.GetGenericTypeDefinition(), implementation);
            RegisterServiceIfNotRegistered(services, lifetime, implementation, implementation);
        }
        else
        {
            RegisterServiceIfNotRegistered(services, lifetime, @interface, implementation);
            RegisterServiceIfNotRegistered(services, lifetime, implementation, implementation);
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

    private static (Type? topBaseType, Type[] interfaces) FindTopBaseType(Type type)
    {
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
