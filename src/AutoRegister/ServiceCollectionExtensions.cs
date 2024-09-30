namespace AutoRegister;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Automatically registers services decorated with <see cref="RegisterAttribute"/> 
    /// from the provided assembly into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    /// <param name="assembly">The assembly to scan for services marked with <see cref="RegisterAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="assembly"/> is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there is an error during the registration process of services from the provided assembly.
    /// </exception>
    /// <remarks>
    /// The specified assembly will be scanned for types that are decorated with the <see cref="RegisterAttribute"/>. 
    /// These types will then be registered in the given <see cref="IServiceCollection"/>. 
    /// If the assembly is null, an <see cref="ArgumentNullException"/> is thrown.
    /// </remarks>
    public static void AddAutoregister(this IServiceCollection services, Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly), "Assembly cannot be null.");

        RegisterAssembly(services, assembly);
    }

    /// <summary>
    /// Automatically registers services decorated with <see cref="RegisterAttribute"/> 
    /// from the provided assemblies into the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to register the services into.</param>
    /// <param name="assemblies">An array of assemblies to scan for services marked with <see cref="RegisterAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="assemblies"/> is null, or when one of the provided assemblies is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there is an error during the registration process of services from any of the provided assemblies.
    /// </exception>
    /// <remarks>
    /// Each assembly provided will be scanned for types that are decorated with the <see cref="RegisterAttribute"/>. 
    /// These types will then be registered in the specified <see cref="IServiceCollection"/>.
    /// If no assemblies are provided or if any assembly is null, an <see cref="ArgumentNullException"/> is thrown.
    /// </remarks>
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
