# Home WiFi QR Code API

A modern ASP.NET Core 8 REST API built with **FastEndpoints** for managing WiFi networks and generating QR codes for easy device connections. Features **AWS SQS** and **Lambda** integration for asynchronous event processing.

## ?? Features

- ? **FastEndpoints** - High-performance endpoint framework
- ?? **JWT Authentication** - Secure token-based authentication
- ? **FluentValidation** - Automatic request validation
- ?? **Swagger/OpenAPI** - Interactive API documentation
- ?? **Entity Framework Core** - SQL Server database
- ?? **QR Code Generation** - WiFi QR codes for easy sharing
- ?? **Bulk Download** - Download multiple QR codes as ZIP
- ?? **AgileMapper** - High-performance object-to-object mapping
- ?? **AWS SQS Integration** - Asynchronous message queuing
- ?? **AWS Lambda Processing** - Serverless event processing

## ??? Architecture

```
.NET 8 API (FastEndpoints)
        ?
        ? Creates WiFi QR Code
        ?
        ??? Save to SQL Server Database
        ?
        ??? Send Message to SQS Queue
                ?
                ? Event Source Mapping
                ?
                ??? AWS Lambda Function
                ?   ?? Process WiFi data
                ?   ?? Send notifications
                ?   ?? Update analytics
                ?   ?? Log events
                ?
                ??? Delete message from queue (automatic)
```

## ?? API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password to get JWT token

### WiFi Networks
- `POST /api/wifi` - Create new WiFi network (sends event to SQS)
- `GET /api/wifi` - Get all WiFi networks
- `GET /api/wifi/{id}` - Get WiFi network by ID
- `GET /api/wifi/{id}/qr` - Download QR code PNG
- `POST /api/wifi/bulk-qr` - Download multiple QR codes as ZIP

## ??? Tech Stack

### Backend
- **.NET 8** - Latest .NET framework
- **FastEndpoints 5.30** - Endpoint-focused architecture
- **Entity Framework Core 8** - ORM for database access
- **SQL Server** - Database
- **FluentValidation** - Request validation
- **AgileMapper** - Fast, flexible object mapping
- **QRCoder** - QR code generation
- **Swagger/NSwag** - API documentation

### AWS Services
- **Amazon SQS** - Message queuing for asynchronous processing
- **AWS Lambda** - Serverless function for event processing
- **CloudWatch** - Logging and monitoring
- **IAM** - Identity and access management

## ?? Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express/LocalDB)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)
- [AWS Account](https://aws.amazon.com/) (for SQS/Lambda features)
- [AWS CLI](https://aws.amazon.com/cli/) (optional, for deployment)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/nreddy-hub/FastAPIHomeWifiQR.git
   cd FastAPIHomeWifiQR
   ```

2. **Update connection string**
   
   Edit `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=HomeWifiQRDb;Encrypt=False;Trusted_Connection=True;"
   }
   ```

3. **Configure AWS (Optional - for SQS/Lambda features)**
   
   **Option A: AWS CLI**
   ```bash
   aws configure
   # Enter your AWS Access Key ID, Secret Access Key, and Region
   ```

   **Option B: Update appsettings.json**
   ```json
   "AWS": {
     "Profile": "default",
     "Region": "us-east-1",
     "SQS": {
       "QueueUrl": "https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue",
       "EnableSqs": true
     }
   }
   ```

   **To disable AWS features in development:**
   ```json
   "AWS": {
     "SQS": {
       "EnableSqs": false
     }
   }
   ```

4. **Install EF Core tools** (if not already installed)
   ```bash
   dotnet tool install --global dotnet-ef --version 8.0.0
   ```

5. **Create database**
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Open Swagger UI**
   
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

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ssid": "MyHomeNetwork",
  "encryption": "WPA2",
  "hidden": false,
  "createdAt": "2024-12-15T10:30:00Z",
  "qrCodeUrl": "/api/wifi/3fa85f64-5717-4562-b3fc-2c963f66afa6/qr"
}
```

? **When SQS is enabled:** A message is automatically sent to AWS SQS for asynchronous processing!

### Download QR Code

```http
GET /api/wifi/{id}/qr
Authorization: Bearer YOUR_TOKEN_HERE
```

Returns a PNG image that devices can scan to connect to the WiFi network.

## ?? AWS Integration

### SQS Message Queue

When a WiFi QR code is created, the API sends an event message to Amazon SQS:

```json
{
  "wifiId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "ssid": "MyHomeNetwork",
  "encryption": "WPA2",
  "hidden": false,
  "createdAt": "2024-12-15T10:30:00Z",
  "createdBy": "admin",
  "metadata": {
    "source": "API",
    "version": "1.0"
  }
}
```

### Lambda Function Processing

The Lambda function automatically processes messages from SQS and can:
- ?? Send email notifications
- ?? Update analytics databases
- ?? Trigger other workflows
- ?? Log events to CloudWatch

### Setup AWS Services

For complete AWS setup instructions, see [AWS SQS Lambda Guide](AWS_SQS_LAMBDA_GUIDE.md)

**Quick Setup:**

1. **Create SQS Queue:**
   ```bash
   aws sqs create-queue --queue-name wifi-qr-queue
   ```

2. **Deploy Lambda Function:**
   ```bash
   cd WifiQrLambdaProcessor
   dotnet lambda deploy-function WifiQrProcessor
   ```

3. **Connect SQS to Lambda:**
   ```bash
   aws lambda create-event-source-mapping \
     --function-name WifiQrProcessor \
     --event-source-arn arn:aws:sqs:REGION:ACCOUNT:wifi-qr-queue
   ```

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

## ?? Project Structure

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
?   ??? ISqsService.cs
?   ??? SqsService.cs
??? Models/
?   ??? WifiNetwork.cs
?   ??? WifiQrCreatedMessage.cs
??? Data/
?   ??? ApplicationDbContext.cs
??? Program.cs
??? appsettings.json
```

## ?? Documentation

- [AgileMapper Guide](AGILE_MAPPER_USAGE.md) - Object mapping examples and best practices
- [AgileMapper Quick Reference](AGILE_MAPPER_QUICK_REFERENCE.md) - Quick mapping patterns
- [AgileMapper Endpoints Summary](AGILE_MAPPER_ENDPOINTS_SUMMARY.md) - Mapping usage across endpoints
- [AWS SQS Lambda Guide](AWS_SQS_LAMBDA_GUIDE.md) - Complete AWS integration guide
- [AWS Quick Reference](AWS_QUICK_REFERENCE.md) - Quick AWS commands and tips
- [AWS Integration Summary](AWS_INTEGRATION_SUMMARY.md) - Integration overview
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
  },
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1",
    "SQS": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue",
      "EnableSqs": true
    }
  }
}
```

?? **Security:** Never commit `appsettings.json` with production secrets to source control!

### Environment-Specific Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": false  // Disable AWS in development
    }
  }
}
```

**Production** (`appsettings.Production.json`):
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": true  // Enable AWS in production
    }
  }
}
```

## ?? Testing

### Using Swagger UI

1. Navigate to http://localhost:5014/swagger
2. Click **Authorize** button
3. Login via `/api/auth/login` to get token
4. Paste token in format: `Bearer YOUR_TOKEN`
5. Test all endpoints with authentication

### Testing AWS Integration

**Monitor SQS Queue:**
```bash
aws sqs get-queue-attributes \
  --queue-url YOUR_QUEUE_URL \
  --attribute-names ApproximateNumberOfMessages
```

**View Lambda Logs:**
```bash
aws logs tail /aws/lambda/WifiQrProcessor --follow
```

**Send Test Message:**
```bash
aws sqs send-message \
  --queue-url YOUR_QUEUE_URL \
  --message-body '{"wifiId":"test-123","ssid":"TestNetwork"}'
```

## ?? NuGet Packages

### Core Packages
- FastEndpoints (5.30.0)
- FastEndpoints.Security (5.30.0)
- FastEndpoints.Swagger (5.30.0)
- Microsoft.EntityFrameworkCore.SqlServer (8.0.0)

### AWS Packages
- AWSSDK.SQS (3.7.x)
- AWSSDK.Extensions.NETCore.Setup (3.7.x)

### Utility Packages
- QRCoder (1.6.0)
- AgileObjects.AgileMapper (1.8.1)

## ?? Monitoring & Observability

### Application Logs
- Structured logging with `ILogger`
- SQS send/receive operations logged
- Error tracking and diagnostics

### AWS CloudWatch
- Lambda execution metrics
- SQS queue metrics
- Custom application metrics

### Key Metrics to Monitor
- **API Response Time**
- **SQS Messages Sent**
- **Lambda Invocations**
- **Lambda Errors**
- **Queue Depth**

## ?? AWS Cost Estimation

**Monthly costs (approximate):**

| Service | Usage | Cost |
|---------|-------|------|
| SQS | 1M requests | $0.40 |
| Lambda | 1M invocations @ 512MB, 1s | $8.35 |
| CloudWatch Logs | 1GB | $0.50 |
| **Total** | | **~$9.25/month** |

*Free tier covers first 1M Lambda requests and 1M SQS requests*

## ?? Deployment

### Deploy API

**Azure App Service:**
```bash
dotnet publish -c Release
# Deploy via Azure Portal or Azure CLI
```

**AWS Elastic Beanstalk:**
```bash
dotnet publish -c Release
eb init
eb create
```

**Docker:**
```bash
docker build -t wifi-qr-api .
docker run -p 5014:8080 wifi-qr-api
```

### Deploy Lambda Function

```bash
cd WifiQrLambdaProcessor
dotnet lambda deploy-function WifiQrProcessor -c Release
```

## ?? Troubleshooting

### SQS Issues

**Messages not sent:**
- Check `EnableSqs` in appsettings.json
- Verify AWS credentials
- Check API logs for errors

**Lambda not triggered:**
- Verify event source mapping
- Check Lambda execution role permissions
- Validate queue has messages

### General Issues

**Database connection fails:**
- Verify SQL Server is running
- Check connection string
- Ensure database exists

**JWT authentication fails:**
- Verify token is not expired
- Check token format: `Bearer {token}`
- Validate JWT secret key

## ?? Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## ?? License

This project is licensed under the MIT License.

## ?? Resources

### .NET & FastEndpoints
- [FastEndpoints Documentation](https://fast-endpoints.com/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [FluentValidation](https://docs.fluentvalidation.net/)

### AWS Resources
- [AWS SQS Documentation](https://docs.aws.amazon.com/sqs/)
- [AWS Lambda .NET](https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html)
- [AWS SDK for .NET](https://docs.aws.amazon.com/sdk-for-net/)

### Other
- [QRCoder](https://github.com/codebude/QRCoder)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)

## ?? Author

GitHub: [@nreddy-hub](https://github.com/nreddy-hub)

---

? If you find this project useful, please consider giving it a star!

?? **Live Demo:** [Coming Soon]

?? **Support:** Open an issue for questions or bug reports
