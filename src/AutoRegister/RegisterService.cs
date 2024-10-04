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
            var (baseType, interfaces) = FindBaseOrInterfaceType(type);

            RegisterSelf(baseType, type, attribute!.Lifetime, services);
            RegisterAbstractOrBase(baseType, type, attribute.Lifetime, services);
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
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
            else
            {
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
        }
    }

    private static void RegisterAbstractOrBase(Type? baseType, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        if (baseType == null) return;

        if (baseType != typeof(object) && baseType.IsClass)
        {
            if (!baseType.IsGenericType && type.IsGenericTypeDefinition)
            {
                // Handle non-generic abstract or base class with a generic implementation.
                var genericImplementation = type.MakeGenericType(typeof(object));
                RegisterServiceIfNotRegistered(services, lifetime, baseType, genericImplementation);
                RegisterServiceIfNotRegistered(services, lifetime, genericImplementation, genericImplementation);
            }
            else if (baseType.IsGenericType && baseType.IsGenericTypeDefinition)
            {
                // Register generic abstract or base class with a generic implementation.
                RegisterServiceIfNotRegistered(services, lifetime, baseType.GetGenericTypeDefinition(), type);
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
            else
            {
                // Register regular non-generic abstract or base class.
                RegisterServiceIfNotRegistered(services, lifetime, baseType, type);
                RegisterServiceIfNotRegistered(services, lifetime, type, type);
            }
        }
    }

    private static void RegisterInterfaces(Type[] interfaces, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        foreach (var @interface in interfaces)
        {
            if (!@interface.IsGenericType && type.IsGenericTypeDefinition)
            {
                // Handle non-generic interface a generic implementation.
                var genericImplementation = type.MakeGenericType(typeof(object));
                RegisterServiceIfNotRegistered(services, lifetime, @interface, genericImplementation);
                RegisterServiceIfNotRegistered(services, lifetime, genericImplementation, genericImplementation);
            }
            else if (@interface.IsGenericType)
            {
                // Handle generic interface with a generic implementation.
                RegisterGenericInterface(services, lifetime, @interface, type);
            }
            else
            {
                // Register regular non-generic interface.
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

    private static (Type? baseType, Type[] interfaces) FindBaseOrInterfaceType(Type type)
    {
        Type baseType = type.BaseType!;

        if (baseType == typeof(object))
        {
            baseType = null!;
        }

        var interfaces = type.GetInterfaces();
        return (baseType, interfaces);
    }
}