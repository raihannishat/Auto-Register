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

            RegisterSelf(type, attribute!.Lifetime, services);
            RegisterAbstractBase(baseType, type, attribute.Lifetime, services);
            RegisterInterfaces(interfaces, type, attribute.Lifetime, services);
        }
    }

    private static void RegisterSelf(Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        // Self-registration only if no interfaces or abstract base classes
        if (!type.IsAbstract && !type.GetInterfaces().Any())
        {
            RegisterServiceIfNotRegistered(services, lifetime, type, type);
        }
    }

    private static void RegisterAbstractBase(Type? baseType, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        if (baseType != null && baseType.IsAbstract && baseType != typeof(object))
        {
            RegisterServiceIfNotRegistered(services, lifetime, baseType, type);
            RegisterServiceIfNotRegistered(services, lifetime, type, type);
        }
    }

    private static void RegisterInterfaces(Type[] interfaces, Type type, ServiceLifetime lifetime, IServiceCollection services)
    {
        foreach (var @interface in interfaces)
        {
            RegisterServiceIfNotRegistered(services, lifetime, @interface, type);
            RegisterServiceIfNotRegistered(services, lifetime, type, type);
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
