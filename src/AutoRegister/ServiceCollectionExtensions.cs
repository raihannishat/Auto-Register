namespace AutoRegister;

public static class ServiceCollectionExtensions
{
    public static void AddAutoregister(this IServiceCollection services, Assembly assembly)
    {
        Register.Setup(services, assembly);
    }
}
