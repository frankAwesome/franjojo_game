# EzAPI - Unity API Integration Package

**EzAPI** is a streamlined Unity package designed to simplify API integration by abstracting away the need to manually handle HTTP requests. With intuitive tools and flexible configuration options, you can connect to and manage APIs effortlessly within your Unity projects.

---

## 🚀 Features

- Call APIs easily using a singleton or API class instance
- Configure and manage endpoints directly from the Unity Editor (`Tools > EzAPI > Settings`)
- Use ScriptableObjects (`APIConfig`) to manage timeout, retry count, content types, and more
- Add headers, parameters, and callback functions with full control
- Inherit from base classes to standardize payload and response formats
- Assign payload and response models per endpoint directly in Settings

---

## 🛠 How to Use

### 1. Calling an API

You can hit an API in two ways:

#### Option 1: Singleton Access

```csharp
APIManager.Instance.HitAPI<YourPayloadType, YourResponseType>(
    "YourEndpointName", 
    payloadObject, 
    OnSuccessCallback, 
    OnErrorCallback
);
```

#### Option 2: Create APIClass Instance

```csharp
var api = new APIClass();
api.HitAPI<YourPayloadType, YourResponseType>(
    "YourEndpointName",
    payloadObject,
    OnSuccessCallback,
    OnErrorCallback,
    headersDict
);
```

This method allows you to customize request headers, manage different response functions, and handle per-call logic more flexibly.

---

## ⚙️ Configuring Endpoints

To set up or manage API endpoints:

1. Open Unity Editor and navigate to `Tools > EzAPI > Settings`
2. Add a new endpoint or edit existing ones
3. For each endpoint:
   - Assign a **Payload** class (must inherit from `RequestPayloadBase`)
   - Assign a **Response** class (must inherit from `RequestResponseBase`)
   - Optionally override default settings like retry count, timeout, or content type

---

## 🧩 Creating an APIConfig

Create reusable configurations for API behavior:

1. Right-click in the Project window
2. Select `Create > EzAPI > APIConfig`
3. Set values such as:
   - Retry count
   - Timeout duration
   - Content type (e.g., `application/json`)
4. Assign this `APIConfig` to endpoints via the Settings window to override global defaults

---

## 📦 Payload and Response Models

To ensure proper serialization and type handling:

- All **request payload classes** must inherit from:

  ```csharp
  RequestPayloadBase
  ```

- All **response classes** must inherit from:

  ```csharp
  RequestResponseBase
  ```

These base classes provide structure and ensure consistency across all API interactions.

---

## ✅ Example

```csharp
public class LoginPayload : RequestPayloadBase {
    public string username;
    public string password;
}

public class LoginResponse : RequestResponseBase {
    public string token;
}

// Using Singleton
APIManager.Instance.HitAPI<LoginPayload, LoginResponse>(/*params*/);

// Or using APIClass
var api = new APIClass();
/*Set other things using api (Like headers, callbacks, payload and response type , params ,etc.)*/
api.HitAPI(/*Params*/);

```

---

## 🧰 Requirements

- Unity 2020.3 or newer
- .NET 4.x Scripting Runtime
- Compatible with URP and HDRP

---

## 📄 License

This package is licensed under the MIT License. See the `LICENSE` file for details.
