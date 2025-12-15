# JWT Authentication Setup - Complete ?

## What Changed

Your API now uses **JWT (JSON Web Token)** authentication instead of API keys.

---

## ?? Authentication Flow

### 1. **Login to Get JWT Token**

**Endpoint:** `POST /api/auth/login`

**Request:**
```json
{
  "username": "admin",
  "password": "password123"
}
```

**Response (Success):**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-15T22:49:36Z"
}
```

**Response (Failed):**
```json
{
  "success": false,
  "message": "Invalid username or password"
}
```

---

### 2. **Use Token for Protected Endpoints**

All WiFi endpoints now require authentication. You must include the JWT token in the `Authorization` header:

**Header:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Testing in Swagger

1. **Login First:**
   - Go to `POST /api/auth/login`
   - Click "Try it out"
   - Enter credentials:
     ```json
     {
       "username": "admin",
       "password": "password123"
     }
     ```
   - Click "Execute"
   - Copy the `token` from the response

2. **Authorize Swagger:**
   - Click the **?? Authorize** button at the top of Swagger UI
   - Enter: `Bearer <your-token>` (include the word "Bearer" followed by a space)
   - Click "Authorize"
   - Click "Close"

3. **Test Protected Endpoints:**
   - Now you can test any WiFi endpoint
   - They will automatically include your JWT token

---

## ?? Protected Endpoints

All these endpoints now require a valid JWT token:

- ? `POST /api/wifi` - Create WiFi network
- ? `GET /api/wifi` - Get all WiFi networks
- ? `GET /api/wifi/{id}` - Get WiFi by ID
- ? `GET /api/wifi/{id}/qr` - Download QR code
- ? `POST /api/wifi/bulk-qr` - Download bulk QR codes

---

## ?? Default Credentials (CHANGE THESE!)

**Username:** `admin`  
**Password:** `password123`

?? **IMPORTANT:** These are hardcoded for demo purposes. You should:
1. Create a User model and database table
2. Hash passwords using BCrypt or similar
3. Validate credentials against the database
4. Add user registration endpoint

---

## ?? JWT Configuration

Located in `appsettings.json`:

```json
"JwtSettings": {
  "SigningKey": "This_Is_A_Super_Secret_Key_That_Should_Be_At_Least_32_Characters_Long!",
  "Issuer": "HomeWifiQR",
  "Audience": "HomeWifiQRUsers",
  "ExpiryMinutes": 60
}
```

**Settings Explained:**
- **SigningKey** - Secret key used to sign tokens (keep this secure!)
- **Issuer** - Who issued the token
- **Audience** - Who can use the token
- **ExpiryMinutes** - Token validity period (60 minutes = 1 hour)

?? **For Production:**
- Store `SigningKey` in environment variables or Azure Key Vault
- Use a strong, randomly generated key
- Never commit secrets to source control

---

## ?? Example: Using the API with Postman/Code

### Step 1: Login
```bash
POST http://localhost:5014/api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

### Step 2: Use Token
```bash
GET http://localhost:5014/api/wifi
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## ?? Security Best Practices

### Next Steps for Production:

1. **User Management:**
   - Create User entity with hashed passwords
   - Add user registration endpoint
   - Store users in database

2. **Password Security:**
   ```bash
   dotnet add package BCrypt.Net-Next
   ```
   - Hash passwords with BCrypt
   - Never store plain text passwords

3. **Token Refresh:**
   - Implement refresh tokens
   - Allow users to get new tokens without re-login

4. **Claims & Roles:**
   - Add more user claims (email, roles, permissions)
   - Implement role-based authorization
   - Example: `Roles("Admin")` in endpoint configuration

5. **HTTPS Only:**
   - Always use HTTPS in production
   - Never send tokens over HTTP

6. **Environment Variables:**
   ```bash
   # In production, use environment variables
   export JwtSettings__SigningKey="your-secret-key"
   ```

---

## ?? Example: Adding Role-Based Authorization

Update an endpoint to require specific role:

```csharp
public override void Configure()
{
    Post("/api/wifi");
    AuthSchemes("Bearer");
    Roles("Admin"); // Only users with "Admin" role can access
}
```

---

## ? What's Working Now

- ? JWT token generation on login
- ? Token validation on all WiFi endpoints
- ? Swagger UI with Bearer token support
- ? 60-minute token expiry
- ? Username and role claims in token

---

## ?? Run Your API

```bash
dotnet run
```

Then visit: **http://localhost:5014/swagger**

---

## ?? Resources

- [FastEndpoints Security Docs](https://fast-endpoints.com/docs/security)
- [JWT.io - Decode & Verify Tokens](https://jwt.io)
- [ASP.NET Core Security](https://learn.microsoft.com/en-us/aspnet/core/security/)

---

Your API is now secured with JWT! ????
