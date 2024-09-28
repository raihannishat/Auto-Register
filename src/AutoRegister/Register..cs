namespace AutoRegister;

public class Register
{
    public static void Setup(IServiceCollection services, Assembly assembly)
    {
        RegisterService.AddServices(services, assembly);
    }
}
