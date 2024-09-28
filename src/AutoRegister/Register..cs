namespace AutoRegister;

internal class Register
{
    internal static void Setup(IServiceCollection services, Assembly assembly)
    {
        RegisterService.AddServices(services, assembly);
    }
}
