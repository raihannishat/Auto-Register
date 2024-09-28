namespace AutoRegister;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RegisterAttribute : Attribute
{
    public ServiceLifetime Lifetime { get; }

    public RegisterAttribute(ServiceLifetime lifetime)
    {
        Lifetime = lifetime;
    }
}
