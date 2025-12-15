# FluentValidation Setup - Complete ?

## Overview

FastEndpoints includes **FluentValidation** built-in! All your request DTOs now have automatic validation.

---

## ?? Validators Created

### 1. **LoginRequestValidator**
Location: `Endpoints/Auth/LoginRequestValidator.cs`

**Rules:**
- ? Username is required
- ? Username must be at least 3 characters
- ? Password is required
- ? Password must be at least 6 characters

**Example Error Response:**
```json
{
  "statusCode": 400,
  "message": "One or more validation errors occurred",
  "errors": {
    "Username": ["Username is required"],
    "Password": ["Password must be at least 6 characters"]
  }
}
```

---

### 2. **CreateWifiRequestValidator**
Location: `Endpoints/Wifi/CreateWifiRequestValidator.cs`

**Rules:**
- ? SSID is required
- ? SSID max length: 32 characters
- ? Password max length: 63 characters (WiFi standard)
- ? Encryption must be: WPA, WPA2, WPA3, WEP, or nopass
- ? Password required for encrypted networks (WPA/WPA2/WPA3/WEP)
- ? Password must be empty for open networks (nopass)

**Smart Validation:**
```json
// ? This will fail - encrypted network without password
{
  "ssid": "MyNetwork",
  "encryption": "WPA2",
  "password": ""
}

// ? This will succeed
{
  "ssid": "MyNetwork",
  "encryption": "WPA2",
  "password": "SecurePassword123"
}

// ? Open network (no password required)
{
  "ssid": "OpenNetwork",
  "encryption": "nopass"
}
```

---

### 3. **GetWifiByIdRequestValidator**
Location: `Endpoints/Wifi/GetWifiByIdRequestValidator.cs`

**Rules:**
- ? WiFi ID is required (non-empty GUID)

---

### 4. **DownloadQrCodeRequestValidator**
Location: `Endpoints/Wifi/DownloadQrCodeRequestValidator.cs`

**Rules:**
- ? WiFi ID is required (non-empty GUID)

---

### 5. **DownloadBulkQrCodesRequestValidator**
Location: `Endpoints/Wifi/DownloadBulkQrCodesRequestValidator.cs`

**Rules:**
- ? At least one WiFi ID is required
- ? Maximum 50 QR codes per request (performance limit)
- ? Each ID must be a valid non-empty GUID

---

## ?? How It Works

FastEndpoints **automatically discovers and applies validators** by naming convention:

```
Request DTO: LoginRequest
Validator: LoginRequestValidator  ? Must inherit from Validator<LoginRequest>
```

No configuration needed! Just create a validator class with the right name.

---

## ?? Testing Validation in Swagger

### Test 1: Empty Username/Password
**Request to `/api/auth/login`:**
```json
{
  "username": "",
  "password": ""
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Username": ["Username is required"],
    "Password": ["Password is required"]
  }
}
```

---

### Test 2: Invalid Encryption Type
**Request to `/api/wifi` (POST):**
```json
{
  "ssid": "TestNetwork",
  "encryption": "INVALID",
  "password": "test123"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Encryption": ["Encryption must be one of: WPA, WPA2, WPA3, WEP, or nopass"]
  }
}
```

---

### Test 3: SSID Too Long
**Request to `/api/wifi` (POST):**
```json
{
  "ssid": "ThisIsAVeryLongSSIDThatExceedsTheMaximumAllowedLengthOf32Characters",
  "encryption": "WPA2",
  "password": "test123"
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Ssid": ["SSID cannot exceed 32 characters"]
  }
}
```

---

### Test 4: Encrypted Network Without Password
**Request to `/api/wifi` (POST):**
```json
{
  "ssid": "SecureNetwork",
  "encryption": "WPA2",
  "password": ""
}
```

**Expected Response (400 Bad Request):**
```json
{
  "errors": {
    "Password": ["Password is required for encrypted networks"]
  }
}
```

---

## ?? Advanced Validation Examples

### Custom Validator with Database Check

```csharp
public class CreateWifiRequestValidator : Validator<CreateWifiRequest>
{
    private readonly ApplicationDbContext _db;

    public CreateWifiRequestValidator(ApplicationDbContext db)
    {
        _db = db;

        RuleFor(x => x.Ssid)
            .NotEmpty()
            .MustAsync(BeUniqueSSID)
            .WithMessage("A network with this SSID already exists");
    }

    private async Task<bool> BeUniqueSSID(string ssid, CancellationToken ct)
    {
        return !await _db.WifiNetworks.AnyAsync(w => w.Ssid == ssid, ct);
    }
}
```

---

### Conditional Validation

```csharp
RuleFor(x => x.Password)
    .MinimumLength(8)
    .When(x => x.Encryption == "WPA3")
    .WithMessage("WPA3 requires password to be at least 8 characters");
```

---

### Complex Rules

```csharp
RuleFor(x => x.Password)
    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")
    .When(x => x.Encryption == "WPA3")
    .WithMessage("Password must contain uppercase, lowercase, and numbers");
```

---

## ?? Validation Benefits

### 1. **Automatic Validation**
- Runs before your endpoint handler
- Returns 400 Bad Request automatically
- No manual validation code needed

### 2. **Clear Error Messages**
- Structured JSON error responses
- Field-specific error messages
- Easy to consume by frontend

### 3. **Type Safety**
- Compile-time checking
- IntelliSense support
- Refactoring-friendly

### 4. **Reusable Rules**
```csharp
// Define once
public static class CommonRules
{
    public static IRuleBuilderOptions<T, string> ValidSSID<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(32)
            .Matches(@"^[a-zA-Z0-9-_]+$")
            .WithMessage("SSID can only contain letters, numbers, hyphens, and underscores");
    }
}

// Use anywhere
RuleFor(x => x.Ssid).ValidSSID();
```

---

## ?? Validation in Action

FastEndpoints validates requests in this order:

1. **Model Binding** - Convert JSON to C# object
2. **Validation** - Run FluentValidation rules
3. **Authorization** - Check JWT token & permissions
4. **Handler** - Your endpoint logic executes

If validation fails, the handler **never runs**.

---

## ?? Testing Validation

### Unit Test Example

```csharp
[Fact]
public async Task LoginRequest_WithEmptyUsername_ShouldFail()
{
    // Arrange
    var validator = new LoginRequestValidator();
    var request = new LoginRequest
    {
        Username = "",
        Password = "password123"
    };

    // Act
    var result = await validator.ValidateAsync(request);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == "Username");
}
```

---

## ? What's Validated

| Endpoint | Request | Validation |
|----------|---------|------------|
| POST /api/auth/login | LoginRequest | Username/Password required, min length |
| POST /api/wifi | CreateWifiRequest | SSID, Password, Encryption validation |
| GET /api/wifi/{id} | GetWifiByIdRequest | Valid GUID required |
| GET /api/wifi/{id}/qr | DownloadQrCodeRequest | Valid GUID required |
| POST /api/wifi/bulk-qr | DownloadBulkQrCodesRequest | IDs required, max 50 limit |

---

## ?? Next Steps

1. **Test the validators** in Swagger
2. **Add custom validation rules** as needed
3. **Write unit tests** for complex validators
4. **Add async validation** for database checks

---

## ?? Resources

- [FluentValidation Docs](https://docs.fluentvalidation.net/)
- [FastEndpoints Validation](https://fast-endpoints.com/docs/validation)
- [Built-in Validators](https://docs.fluentvalidation.net/en/latest/built-in-validators.html)

---

All validation is now working automatically! ???
