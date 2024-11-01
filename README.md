<link rel="shortcut icon" type="image/x-icon" href="[favicon.ico](https://github.com/raihannishat/Auto-Register/blob/main/icon.png)">

# Auto-Register
## Overview
<div align="center">
  <img src="https://github.com/raihannishat/Auto-Register/blob/main/icon.png?raw=true" alt="Package Icon"/>
</div>
<br>

The **Auto-Register** package simplifies service registration for ASP.NET Core applications by automatically discovering and registering services based on custom attributes. This package eliminates the need for manually adding services in Program.cs, supports multiple service lifetimes, and ensures no duplicate registrations occur.

With Auto-Register, services are identified using the RegisterAttribute and are automatically registered as self, interface, or base class implementations.

[Nuget] : https://www.nuget.org/packages/Auto-Register

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

If using **ASP.NET Core 5.0+** with a minimal hosting model (Program.cs), add the auto-registration in the ConfigureServices section

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

// Program.cs (ASP.NET Core 5.0+)
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
Classes can be registered not only as themselves but also as their **interfaces, abstract or any base** class.

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

## Supported Platforms
Auto-Register is designed for use any types of .NET applications that support Microsoft Dependency Injection, including:

* ASP.NET Core Applications
* Worker Services
* Blazor Applications
* WPF Applications
* Windows Forms Applications

I am showing some examples through a **console** application

### Advanced Generic Service Processing with Auto Registration in ASP.NET Core
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var processor = serviceProvider.GetRequiredService<IMyProcess>();
processor.Save(new Customer());

public interface IEntity
{
    Guid Id { get; }
}

public interface IRepository<T> where T : IEntity
{
    void Save(T entity);
}

[Register(ServiceLifetime.Transient)]
public class Repository<T> : IRepository<T> where T : IEntity, new()
{
    public void Save(T entity)
    {
        Console.WriteLine($"Entity with ID {entity.Id} has been saved.");
    }
}

public interface IService<T> where T : IEntity
{
    void Process(T entity);
}

[Register(ServiceLifetime.Transient)]
public class Service<T> : IService<T> where T : IEntity, new()
{
    private readonly IRepository<T> _repository;

    public Service(IRepository<T> repository)
    {
        _repository = repository;
    }

    public void Process(T entity)
    {
        Console.WriteLine($"Processing entity with ID {entity.Id}...");
        _repository.Save(entity);
    }
}

public class Customer : IEntity
{
    public Guid Id { get; private set; }

    public Customer()
    {
        Id = Guid.NewGuid();
    }
}

[Register(ServiceLifetime.Transient)]
public class Processor<TRepository, TService, TEntity>
    where TRepository : IRepository<TEntity>
    where TService : IService<TEntity>
    where TEntity : IEntity, new()
{
    private readonly TRepository _repository;
    private readonly TService _service;

    public Processor(TRepository repository, TService service)
    {
        _repository = repository;
        _service = service;
    }

    public void Execute()
    {
        TEntity entity = new TEntity();
        Console.WriteLine($"Executing for entity with ID {entity.Id}...");
        _service.Process(entity);
    }
}

[Register(ServiceLifetime.Transient)]
public class AdvanceProcess : Processor<Repository<Customer>, Service<Customer>, Customer>, IMyProcess
{
    public AdvanceProcess(Repository<Customer> repository, Service<Customer> service) 
        : base(repository, service)
    {
    }

    public void Save(Customer entity)
    {
        Console.WriteLine($"Entity with ID {entity.Id} and the type is {entity.GetType().FullName}");
    }
}

public interface IMyProcess
{
    void Save(Customer entity);
}
```
#### [Output] : Entity with ID 3ccac989-03bc-4a17-9ae1-ff444140b185 and the type is Customer

##
### Here are a few more examples that expand on the Advanced Generic Service Processing model with auto-registration and dependency injection in ASP.NET Core.
##

### Example 1: Simple Open Generic Interface and Class
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
#### [Output] : Executed Scoped Service with type System.Int32
##
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
#### [Output] : Processing Closed Generic Service with int 99
##
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
#### [Output] : Executed Transient with 5 and Auto-Register
##
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
#### [Output] : Non-generic service executed (Singleton)
##
### Example 5: Closed Generic Class with Concrete Types
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
#### [Output] : Executed with closed generic type: System.Int32
##
### Example 6: Complex Generic Class with Constraints
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
#### [Output] : Executed Singleton with System.Int32
##
### Example 7: Generic with Multiple Constraints
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
#### [Output] : Executed Scoped with System.Int32 and constraint
##
### Example 8: Nested Generics
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
#### [Output] : Executed Transient with nested System.Collections.Generic.List`1[System.Int32]
##
### Example 9: Hybrid Non-Generic and Generic Services
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
#### [Output] : 
#### Executing Scoped HybridService.
#### Executing Scoped OpenGenericService with type: System.Int32.
##
### Example 10: Multi-Layer Inheritance (Non-Generic and Generic)
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
#### [Output] : 
#### Executing Singleton LayeredService.
#### Executing Singleton GenericBaseService with type: System.Int32
##
### Example 11: Multiple Implementations of Interface Non-Generic and Generic Services
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myService = serviceProvider.GetRequiredService<IServiceFactory>()
    .GetService("A");

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
    private readonly IServiceProvider _serviceProvider;

    public ServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IService GetService(string implementationType)
    {
        return implementationType switch
        {
            "A" => _serviceProvider.GetRequiredService<ServiceImplementationA>(),
            "B" => _serviceProvider.GetRequiredService<ServiceImplementationB>(),
            _ => throw new ArgumentException($"Service type {implementationType} is not supported.")
        };
    }
}

public interface IServiceFactory
{
    IService GetService(string implementationType);
}
```
#### [Output] : Executing Scoped ServiceImplementationA with type: System.String.
##
### Example 12: Complex Generic Class with inheritance and interface implementation
```csharp
using AutoRegister;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

var services = new ServiceCollection();
services.AddAutoregister(Assembly.GetExecutingAssembly());
using var serviceProvider = services.BuildServiceProvider();

var myCar = serviceProvider.GetRequiredService<ElectricCar>();
myCar.Drive();

[Register(ServiceLifetime.Scoped)]
public class Vehicle
{
    public virtual void StartEngine()
    {
        Console.WriteLine("Engine started.");
    }
}

public abstract class LandVehicle : Vehicle
{
    public abstract void Drive();
}

public interface ITransport
{
    void GetTransportMode();
}

[Register(ServiceLifetime.Scoped)]
public class Car<T> : LandVehicle, ITransport where T : Vehicle, new()
{
    public override void Drive()
    {
        Console.WriteLine("Driving on the road.");
    }

    public void GetTransportMode()
    {
        Console.WriteLine("Land transport");
    }

    public override void StartEngine()
    {
        Console.WriteLine("Car engine started.");
    }
}

[Register(ServiceLifetime.Scoped)]
public class ElectricCar : Car<ElectricCar>
{
    public override void Drive()
    {
        Console.WriteLine("Driving silently on electric power.");
    }
}
```
#### [Output] : Driving silently on electric power.
##
## Conclusion
The **Auto-Register** NuGet package provides a powerful and flexible way to manage service registration in ASP.NET Core. By automating service discovery and registration, it reduces boilerplate code and helps maintain clean and maintainable service registration logic, especially in large projects with many services.
