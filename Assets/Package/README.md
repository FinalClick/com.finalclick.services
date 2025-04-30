
# FinalClick - Services
**Unity Package: `com.finalclick.services`**

A lightweight and flexible service registration system for Unity, designed to simplify global and scene-specific service management through automatic discovery and structured lifecycles.

---

## 🚀 Getting Started in Seconds

Register a service with just one attribute:

```csharp
[RegisterAsService]
public class MyService : MonoBehaviour
{
}
```
or
```csharp
[RegisterServices]
public static void Register(ServiceCollectionBuilder builder)
{
    builder.Register<IMyService, MyService>();
}
```

Then access it with:

```csharp
var myService = gameObject.Get<MyService>();
```

Or inject it directly into other services:

```csharp
[InjectService]
private IMyService MyService { get; } = null!;
```

FinalClick.Services takes care of discovery, lifecycle, and injection—**so you can focus on your game logic.**


---

## Overview

`FinalClick.Services` allows you to register and resolve services both at the application level and per scene. The system is designed to integrate naturally with Unity's Play Mode lifecycle and scene loading events.

Services can be registered using static methods, allowing you to create pure csharp services, or via MonoBehaviours which lets you leverage references to assets or scene objects.


---

## Features

- 🔍 **Automatic Service Discovery** using the `RegisterServices` and `RegisterAsService` attributes.
- 💡 **Global Application Services** via `ApplicationServices`.
- 🎬 **Scene-scoped Services** via `SceneServices`.
- 🔗 **Inspector Unity Object Reference Support** when using MonoBehaviour services in scenes or application services prefab.
- ⚙️ **Lifecycle Hooks** via the `IService` interface.
- 🚀 **Disable Domain Reload Support**: automatic start and stops services during scene load and unload, and when entering or exiting Play Mode.
- 🧩 Automatic Dependency Injection via the InjectService attribute. Inject services directly into properties via reflection.

---

## Application Services

### Registering Services

To register services at the application level, accessible from anywhere in code, create a static method marked with the `[RegisterServices]` attribute.

Example:

```csharp
[RegisterServices]
public static void RegisterMyServices(ServiceCollectionBuilder builder)
{
    builder.Register<IMyService, MyService>();
}
```

- The method must be `static`.
- It must accept exactly **one parameter**: `ServiceCollectionBuilder`.
- These methods are automatically called **before the first scene is loaded**.

Or, using a Application Services Prefab, use the `[RegisterServices]` (on a member function) or `[RegisterAsService]` (on the class) on a root MonoBehavior on the prefab:

```csharp
[RegisterAsService]
public class MyService : MonoBehavour
{
    ///
}
```
or
```csharp
public class OtherService : MonoBehavour
{
    [RegisterServices]
    private void MyRegisterFunction(ServiceCollectionBuilder builder)
    {
        builder.Register<OtherService>(OtherService);
    }
}
```
And assign a application services prefab in the `Project Settings/ServiceSettings`.
- Any `[RegisterServices]` methods on components on the prefab will be called with the application service collection builder.
- Any `[RegisterServiceAs(Type[])]` components on the prefab will be automatically registered.

This prefab will be automatically injected into the first scene that's loaded.

---

### Accessing Application Services

Once registered, you can resolve services using:

```csharp
var service = ApplicationServices.Get<IMyService>();
```

or safely check:

```csharp
if (ApplicationServices.TryGet<IMyService>(out var service))
{
    service.DoSomething();
}
```

---

## Scene Service Registration

Scene services can be registered via MonoBehaviours.

#### For Scene Scope Services

They can be created the same way ApplicationSerivces can, but instead of being on the application prefab they must be on a root GameObject in the scene.

1. Any `[RegisterServices]` methods on a root GameObjects MonoBehaviour will be called on scene load. 
2. Any `[RegisterAsService]` MonoBehaviours will be registered on scene load, IF on a root GameObject in the scene.

---

### Accessing Scene Services

> Note, the `InjectService` attribute can only be used if the **MonoBehaviour** or csharp **class** is a registered service.

You can retrieve services registered to a specific scene using:

via `MonoBehaviour` and `GameObject` extension methods. These automatically check the scene scope serivces for their scene before getting the service from the ApplicationServices.

```csharp
public class ExampleComponent : MonoBehavour
{
    [InjectService] 
    private IMyService MyService { get; } = null;

    // OR
    
    void Awake()
    {
        gameobject.GetService<IMyService>();
    }
}
```

or via the Static class if working outside of unity code.

```csharp
var service = SceneServices.Get<IMySceneService>(gameObject.scene);
```

or safely:

```csharp
if (SceneServices.TryGet<IMySceneService>(gameObject.scene, out var service))
{
    service.DoSomething();
}
```

This allows services to be uniquely scoped to a scene instance, enabling scene-specific data, references, and logic separation.

---

## Service Lifecycle (`IService`)

For services that need structured startup, update, and shutdown behavior, implement the `IService` interface:

```csharp
namespace FinalClick.Services
{
    public interface IService
    {
        void OnServiceStart();
        void OnServiceUpdate();
        void OnServiceStop();
    }
}
```

| Method             | Application Service Called When | Scene Service Called When |
|---------------------|-------------------|---------------------------|
| `OnServiceStart()`  | On Application startup | On Scene Loaded           |
| `OnServiceUpdate()` | Once per frame    | Once per frame            |
| `OnServiceStop()`   | On Application shutdown | On Scene Unloaded         |

---

## Lifecycle Overview

| Event                             | Action                                             |
|-----------------------------------|---------------------------------------------------|
| Entering Play Mode                | Application services are registered and started.  |
| Loading a Scene                   | Scene services are registered and started.        |
| Unloading a Scene                 | Scene services are stopped.                       |
| Exiting Play Mode / Application Quit | Application and scene services are stopped.      |

---
