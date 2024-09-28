# Auto-Register
## Overview
![Package Icon](https://github.com/raihannishat/Auto-Register/blob/main/src/AutoRegister/icon.png?raw=true) <br/>

The **Auto-Register** package simplifies service registration for ASP.NET Core applications by automatically discovering and registering services based on custom attributes. This package eliminates the need for manually adding services in Startup.cs, supports multiple service lifetimes, and ensures no duplicate registrations occur.

With AutoRegister, services are identified using the RegisterAttribute and are automatically registered as self, interface, or base class implementations.

### Installation
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

var builder = WebApplication.CreateBuilder(args);

// Automatically register services marked with the RegisterAttribute
builder.Services.AddAutoregister(Assembly.GetExecutingAssembly());

var app = builder.Build();
app.Run();
```

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
Classes can be registered not only as themselves but also as their **interfaces and abstract** base classes.

### 3. Self-Registration:
Classes that do not implement interfaces or inherit from abstract base classes can still be **self-registered** in the service collection.

### 4. Duplicate Prevention:
Services are **registered only once**, preventing multiple registrations of the same type.

### 5. Lifetime Control:
Service lifetime is controlled via the **RegisterAttribute**, making it easy to specify whether a service should be **Singleton, Scoped, or Transient**

## Advanced Usage
### Registering External Assemblies
If you want to register services from multiple assemblies, you can pass those assemblies to the **AddAutoregister** method. For example:

```csharp
using AutoRegister;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Register services from multiple assemblies
builder.Services.AddAutoregister(Assembly.GetExecutingAssembly());
builder.Services.AddAutoregister(typeof(SomeExternalService).Assembly);

var app = builder.Build();
app.Run();
```

### Ignoring Specific Services
Currently, all services marked with the **RegisterAttribute** in the provided assemblies will be registered. If you want to exclude certain services, you would need to manually intervene before the registration process.

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

## Conclusion
The **Auto-Register** NuGet package provides a powerful and flexible way to manage service registration in ASP.NET Core. By automating service discovery and registration, it reduces boilerplate code and helps maintain clean and maintainable service registration logic, especially in large projects with many services.
