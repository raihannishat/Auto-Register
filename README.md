# Auto-Register
## Overview
![Package Icon](https://github.com/raihannishat/Auto-Register/blob/main/src/AutoRegister/icon.png?raw=true) <br/>

The **Auto-Register** package simplifies service registration for ASP.NET Core applications by automatically discovering and registering services based on custom attributes. This package eliminates the need for manually adding services in Program.cs, supports multiple service lifetimes, and ensures no duplicate registrations occur.

With Auto-Register, services are identified using the RegisterAttribute and are automatically registered as self, interface, or base class implementations.

## Installation
To install **Auto-Register**, add it to your project using the NuGet Package Manager or .NET CLI:
#### Using Package Manager:
```bash
Install-Package Auto-Register
```

#### Using .NET CLI:
```bash
dotnet add package Auto-Register
```

## Usage

Once the **Auto-Register** package is installed, you can easily use it in your ASP.NET Core project to auto-register services.

### Step 1: Mark Services with RegisterAttribute
Services that need to be registered must be decorated with the RegisterAttribute. This attribute takes the ServiceLifetime **(Singleton, Scoped, or Transient)** as a parameter.

### Example:
```csharp
using AutoRegister;

// Singleton service
[Register(ServiceLifetime.Singleton)]
public class MySingletonService : IMySingletonService
{
    // Implementation
}

// Scoped service
[Register(ServiceLifetime.Scoped)]
public class MyScopedService : IMyScopedService
{
    // Implementation
}

// Transient service
[Register(ServiceLifetime.Transient)]
public class MyTransientService : IMyTransientService
{
    // Implementation
}
```

### Step 2: Register Services in Program.cs
In your ASP.NET Core application, use the **AddAutoregister** extension method to automatically register services from a given assembly.

If using **ASP.NET Core 6.0+** with a minimal hosting model (Program.cs), add the auto-registration in the ConfigureServices section

```csharp
using AutoRegister;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Automatically register services marked with the RegisterAttribute
builder.Services.AddAutoregister(Assembly.GetExecutingAssembly());

var app = builder.Build();
app.Run();
```

## Example Scenario
Consider an ASP.NET Core application where you want to automatically register several services:

```csharp
[Register(ServiceLifetime.Singleton)]
public class AuthService : IAuthService
{
    // Singleton service for authentication
}

[Register(ServiceLifetime.Scoped)]
public class ShoppingCartService : IShoppingCartService
{
    // Scoped service for managing shopping carts
}

[Register(ServiceLifetime.Transient)]
public class PaymentService : IPaymentService
{
    // Transient service for handling payments
}

// Program.cs (ASP.NET Core 6+)
var builder = WebApplication.CreateBuilder(args);

// Automatically register services
builder.Services.AddAutoregister(Assembly.GetExecutingAssembly());

var app = builder.Build();
app.Run();
```

### In this example:
**AuthService** will be registered as a **Singleton** <br/>
**ShoppingCartService** will be registered as a **Scoped** <br/>
**PaymentService** will be registered as a **Transient** <br/>

The services will automatically be resolved and injected where required, without needing to manually specify them in Program.cs

## Components
### RegisterAttribute
This attribute is used to mark classes for automatic registration. It defines the lifetime of the service **(Singleton, Scoped, or Transient)** via the constructor.

### Constructor:
```csharp
public RegisterAttribute(ServiceLifetime lifetime)
```

### Parameter:
**ServiceLifetime** lifetime: Specifies the lifetime of the service to be registered **(Singleton, Scoped, or Transient)**

### Example:
```csharp
[Register(ServiceLifetime.Singleton)]
public class MyService : IMyService
{
    // Implementation
}
```

## Key Features
### 1. Automatic Service Discovery and Registration:
Services marked with the RegisterAttribute are automatically discovered and registered based on their lifetime **(Singleton, Scoped, or Transient)**

### 2. Interface and Base Class Registration:
Classes can be registered not only as themselves but also as their **interfaces, abstract or any base** classes.

### 3. Self-Registration:
Classes that do not implement interfaces or inherit from abstract base classes can still be **self-registered** in the service collection.

### 4. Duplicate Prevention:
Services are **registered only once**, preventing multiple registrations of the same type.

### 5. Lifetime Control:
Service lifetime is controlled via the **RegisterAttribute**, making it easy to specify whether a service should be **Singleton, Scoped, or Transient**

## Advanced Usage
### Registering External Assemblies
If you want to register services from **multiple assemblies**, you can pass those assemblies to the **AddAutoregister** method. For example:

```csharp
using AutoRegister;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Register services from multiple assemblies
builder.Services.AddAutoregister(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
app.Run();
```

### Ignoring Specific Services
Currently, all services marked with the **RegisterAttribute** in the provided assemblies will be registered. If you want to exclude certain services, you would need to manually intervene before the registration process.

### Example 1: Simple Generic Interface and Class
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<int>>();
myService.DoExecute();

public interface IMyService<T>
{
    void DoExecute();
}

[Register(ServiceLifetime.Scoped)]
public class MyService<T> : IMyService<T>
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed Scoped Service with type {typeof(T)}");
    }
}
```
[Output] : Executed Scoped Service with type System.Int32

### Example 2: Closed Generic Interface and Class
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IClosedGenericService<int>>();
myService.Process(99);

public interface IClosedGenericService<T>
{
    void Process(T type);
}

[Register(ServiceLifetime.Scoped)]
public class ClosedGenericService : IClosedGenericService<int>
{
    public void Process(int type)
    {
        Console.WriteLine($"Processing Closed Generic Service with int {type}");
    }
}
```
[Output] : Processing Closed Generic Service with int 99

### Example 3: Multiple Generic Types
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<int, string>>();
myService.DoExecute(5, "Auto-Register");

public interface IMyService<T1, T2> 
{
    void DoExecute(T1 type1, T2 type2); 
}

[Register(ServiceLifetime.Transient)]
public class MyService : IMyService<int, string>
{
    public void DoExecute(int type1, string type2)
    {
        Console.WriteLine($"Executed Transient with {type1} and {type2}");
    }
}
```
[Output] : Executed Transient with 5 and Auto-Register

### Example 4: Non-Generic Interface and Class
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService>();
myService.DoExecute();

public interface IMyService 
{
    void DoExecute(); 
}

[Register(ServiceLifetime.Singleton)]
public class MyService : IMyService
{
    public void DoExecute()
    {
        Console.WriteLine("Non-generic service executed (Singleton)");
    }
}
```
[Output] : Non-generic service executed (Singleton)

### Example 5: Open Generic Interface and Class
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<int>>();
myService.DoExecute();

public interface IMyService<T>
{
    void DoExecute();
}

[Register(ServiceLifetime.Scoped)]
public class MyService<T> : IMyService<T>
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed Scoped with {typeof(T)}");
    }
}
```
[Output] : Executed Scoped with System.Int32

### Example 6: Closed Generic Class with Concrete Types
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService>();
myService.DoExecute();

public interface IMyService 
{
    void DoExecute(); 
}

[Register(ServiceLifetime.Scoped)]
public class MyService : MyGenericService<int>, IMyService
{
}

[Register(ServiceLifetime.Scoped)]
public class MyGenericService<T>
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed with closed generic type: {typeof(T)}");
    }
}
```
[Output] : Executed with closed generic type: System.Int32

### Example 7: Complex Generic Class with Constraints
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<int>>();
myService.DoExecute();

public interface IMyService<T> where T : struct 
{ 
    void DoExecute(); 
}

[Register(ServiceLifetime.Singleton)]
public class MyService<T> : IMyService<T> where T : struct
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed Singleton with {typeof(T)}");
    }
}
```
[Output] : Executed Singleton with System.Int32

### Example 8: Generic with Multiple Constraints
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<int>>();
myService.DoExecute();

public interface IMyService<T> where T : struct, IComparable 
{ 
    void DoExecute(); 
}

[Register(ServiceLifetime.Scoped)]
public class MyService<T> : IMyService<T> where T : struct, IComparable
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed Scoped with {typeof(T)} and constraint");
    }
}
```
[Output] : Executed Scoped with System.Int32 and constraint

### Example 9: Nested Generics
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IMyService<List<int>>>();
myService.DoExecute();

public interface IMyService<T> 
{ 
    void DoExecute(); 
}

[Register(ServiceLifetime.Transient)]
public class MyService<T> : IMyService<T>
{
    public void DoExecute()
    {
        Console.WriteLine($"Executed Transient with nested {typeof(T)}");
    }
}
```
[Output] : Executed Transient with nested System.Collections.Generic.List`1[System.Int32]

### Example 10: Hybrid Non-Generic and Generic Services
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<HybridService>();
myService.DoExecute<int>();

[Register(ServiceLifetime.Scoped)]
public class HybridService
{
    private readonly OpenGenericService<int> _openGenericService;

    public HybridService(OpenGenericService<int> openGenericService)
    {
        _openGenericService = openGenericService;
    }

    public void DoExecute<T>()
    {
        Console.WriteLine("Executing Scoped HybridService.");
        _openGenericService.DoExecute();
    }
}

[Register(ServiceLifetime.Scoped)]
public class OpenGenericService<T>
{
    public void DoExecute()
    {
        Console.WriteLine($"Executing Scoped OpenGenericService with type: {typeof(T)}.");
    }
}
```
[Output] : 
Executing Scoped HybridService.
Executing Scoped OpenGenericService with type: System.Int32.

### Example 11: Multy-Layer Inheritance (Non-Generic and Generic)
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<LayeredService>();
myService.DoExecute<int>();

public abstract class BaseService
{
    public abstract void DoExecute<T>();
}

[Register(ServiceLifetime.Singleton)]
public class GenericBaseService<U> : BaseService
{
    public override void DoExecute<T>()
    {
        Console.WriteLine("Executing Singleton GenericBaseService with type: " + typeof(T));
    }
}

[Register(ServiceLifetime.Singleton)]
public class LayeredService : GenericBaseService<int>
{
    public override void DoExecute<T>()
    {
        Console.WriteLine("Executing Singleton LayeredService.");
        base.DoExecute<T>();
    }
}
```
[Output] : 
Executing Singleton LayeredService.
Executing Singleton GenericBaseService with type: System.Int32

### Example 12: Multiple Implementations Abstract and Interface Non-Generic and Generic Services (Scoped)
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IServiceFactory>().CreateService();
myService.DoExecute<string>();

public interface IService
{
    void DoExecute<T>();
}

[Register(ServiceLifetime.Scoped)]
public class ServiceImplementationA : IService
{
    public void DoExecute<T>()
    {
        Console.WriteLine($"Executing Scoped ServiceImplementationA with type: {typeof(T)}.");
    }
}

[Register(ServiceLifetime.Scoped)]
public class ServiceImplementationB : IService
{
    public void DoExecute<T>()
    {
        Console.WriteLine($"Executing Scoped ServiceImplementationB with type: {typeof(T)}.");
    }
}

[Register(ServiceLifetime.Scoped)]
public class ServiceFactory : IServiceFactory
{
    public IService CreateService() => new ServiceImplementationA();
}

public interface IServiceFactory
{
    IService CreateService();
}
```
[Output] : Executing Scoped ServiceImplementationA with type: System.String.

## Conclusion
The **Auto-Register** NuGet package provides a powerful and flexible way to manage service registration in ASP.NET Core. By automating service discovery and registration, it reduces boilerplate code and helps maintain clean and maintainable service registration logic, especially in large projects with many services.
