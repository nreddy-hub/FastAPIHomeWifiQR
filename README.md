# Home WiFi QR Code API

A modern ASP.NET Core 8 REST API built with **FastEndpoints** for managing WiFi networks and generating QR codes for easy device connections.

## ?? Features

- ? **FastEndpoints** - High-performance endpoint framework
- ? **JWT Authentication** - Secure token-based authentication
- ? **FluentValidation** - Automatic request validation
- ? **Swagger/OpenAPI** - Interactive API documentation
- ? **Entity Framework Core** - SQL Server database
- ? **QR Code Generation** - WiFi QR codes for easy sharing
- ? **Bulk Download** - Download multiple QR codes as ZIP

## ?? API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password to get JWT token

### WiFi Networks
- `POST /api/wifi` - Create new WiFi network
- `GET /api/wifi` - Get all WiFi networks
- `GET /api/wifi/{id}` - Get WiFi network by ID
- `GET /api/wifi/{id}/qr` - Download QR code PNG
- `POST /api/wifi/bulk-qr` - Download multiple QR codes as ZIP

## ??? Tech Stack

- **.NET 8** - Latest .NET framework
- **FastEndpoints 5.30** - Endpoint-focused architecture
- **Entity Framework Core 8** - ORM for database access
- **SQL Server** - Database
- **FluentValidation** - Request validation
- **QRCoder** - QR code generation
- **Swagger/NSwag** - API documentation

## ?? Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express/LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR-USERNAME/FastAPIHomeWifiQR.git
   cd FastAPIHomeWifiQR
   ```

2. **Update connection string**
   
   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=HomeWifiQRDb;Encrypt=False;Trusted_Connection=True;"
   }
   ```

3. **Install EF Core tools** (if not already installed)
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.0
   ```

4. **Create database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Open Swagger UI**
   
   Navigate to: http://localhost:5014/swagger

## ?? Authentication

The API uses JWT (JSON Web Token) authentication.

### Login

**Request:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "password123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-12-15T23:49:36Z"
}
```

### Using the Token

Include the token in the `Authorization` header for all protected endpoints:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Default Credentials

?? **For development only:**
- Username: `admin`
- Password: `password123`

**Important:** Change these in production!

## ?? Example Usage

### Create WiFi Network

```http
POST /api/wifi
Authorization: Bearer YOUR_TOKEN_HERE
Content-Type: application/json

{
  "ssid": "MyHomeNetwork",
  "password": "SecurePassword123",
  "encryption": "WPA2",
  "hidden": false
}
```

### Download QR Code

```http
GET /api/wifi/{id}/qr
Authorization: Bearer YOUR_TOKEN_HERE
```

Returns a PNG image that devices can scan to connect to the WiFi network.

## ? Validation Rules

### WiFi Networks
- SSID: Required, max 32 characters
- Password: 8-63 characters (required for encrypted networks)
- Encryption: WPA, WPA2, WPA3, WEP, or nopass
- Password required for encrypted networks
- Password must be empty for open networks

### Bulk Download
- Minimum: 1 network
- Maximum: 50 networks per request

## ??? Project Structure

```
FastAPIHomeWifiQR/
??? Endpoints/
?   ??? Auth/
?   ?   ??? LoginEndpoint.cs
?   ?   ??? LoginRequest.cs
?   ?   ??? LoginResponse.cs
?   ?   ??? LoginRequestValidator.cs
?   ??? Wifi/
?       ??? CreateWifiEndpoint.cs
?       ??? GetAllWifiEndpoint.cs
?       ??? GetWifiByIdEndpoint.cs
?       ??? DownloadQrCodeEndpoint.cs
?       ??? DownloadBulkQrCodesEndpoint.cs
?       ??? [Validators...]
??? Services/
?   ??? WifiService.cs
?   ??? QrCodeService.cs
?   ??? AgileMapperAdapter.cs
??? Models/
?   ??? WifiNetwork.cs
??? Data/
?   ??? ApplicationDbContext.cs
??? Program.cs
```

## ?? Documentation

- [Migration Guide](MIGRATION_COMPLETE.md) - Controller to FastEndpoints migration
- [JWT Authentication Guide](JWT_AUTHENTICATION_GUIDE.md) - JWT setup and usage
- [Validation Guide](FLUENT_VALIDATION_GUIDE.md) - FluentValidation examples

## ?? Configuration

Edit `appsettings.json` to configure:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your SQL Server connection string"
  },
  "JwtSettings": {
    "SigningKey": "Your-Secret-Key-Here",
    "Issuer": "HomeWifiQR",
    "Audience": "HomeWifiQRUsers",
    "ExpiryMinutes": 60
  }
}
```

?? **Security:** Never commit `appsettings.json` with production secrets to source control!

## ?? Testing

Use Swagger UI for interactive testing:

1. Navigate to http://localhost:5014/swagger
2. Click **Authorize** button
3. Login via `/api/auth/login` to get token
4. Paste token in format: `Bearer YOUR_TOKEN`
5. Test all endpoints with authentication

## ?? NuGet Packages

- FastEndpoints (5.30.0)
- FastEndpoints.Security (5.30.0)
- FastEndpoints.Swagger (5.30.0)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)
- QRCoder (1.6.0)
- AgileObjects.AgileMapper (1.8.1)

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ?? License

This project is licensed under the MIT License.

## ?? Resources

- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [FluentValidation](https://docs.fluentvalidation.net/)
- [QRCoder](https://github.com/codebude/QRCoder)

## ?? Author

Your Name - [@YourGitHub](https://github.com/YOUR-USERNAME)

---

? If you find this project useful, please consider giving it a star!
