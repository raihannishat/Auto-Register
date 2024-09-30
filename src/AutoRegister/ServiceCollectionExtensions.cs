namespace AutoRegister;

public static class ServiceCollectionExtensions
{
    //
    // Summary:
    //     Registers services from the specified assembly into the provided Microsoft.Extensions.DependencyInjection.IServiceCollection.
    //     The assembly is scanned for types marked with RegisterAttribute for automatic registration.
    //
    public static void AddAutoregister(this IServiceCollection services, Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly), "Assembly cannot be null.");

        RegisterAssembly(services, assembly);
    }

    //
    // Summary:
    //     Registers services from the provided assemblies into the specified Microsoft.Extensions.DependencyInjection.IServiceCollection.
    //     Each assembly is scanned for types marked with RegisterAttribute for automatic registration.
    //     At least one assembly must be provided for registration, and null assemblies are not allowed.
    //
    public static void AddAutoregister(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            throw new ArgumentNullException(nameof(assemblies), "At least one assembly must be provided for auto-registration.");

        foreach (var assembly in assemblies)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), "Assembly cannot be null.");

            RegisterAssembly(services, assembly);
        }
    }

    private static void RegisterAssembly(IServiceCollection services, Assembly assembly)
    {
        try
        {
            Register.Setup(services, assembly);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to register services from assembly: {assembly.FullName}.", ex);
        }
    }
}
