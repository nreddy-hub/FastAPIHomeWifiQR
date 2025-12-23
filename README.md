# Home WiFi QR Code API

A modern ASP.NET Core 8 REST API built with **FastEndpoints** for managing WiFi networks and generating QR codes for easy device connections. Features **AWS SQS** and **Lambda** integration for asynchronous event processing.

## Features

- **FastEndpoints** - High-performance endpoint framework
- **JWT Authentication** - Secure token-based authentication
- **FluentValidation** - Automatic request validation
- **Swagger/OpenAPI** - Interactive API documentation
- **Entity Framework Core** - SQL Server database
- **QR Code Generation** - WiFi QR codes for easy sharing
- **Bulk Download** - Download multiple QR codes as ZIP
- **AgileMapper** - High-performance object-to-object mapping
- **AWS SQS Integration** - Asynchronous message queuing
- **AWS Lambda Processing** - Serverless event processing

## Architecture

```
+---------------------------------------+
|   .NET 8 API (FastEndpoints)         |
+---------------------------------------+
                |
                | Creates WiFi QR Code
                |
    +-----------+-----------+
    |                       |
    v                       v
SQL Server            AWS SQS Queue
Database              (Message Queue)
    |                       |
    |                       | Event Source Mapping
    |                       |
    |                       v
    |               AWS Lambda Function
    |                       |
    |           +-----------+-----------+
    |           |           |           |
    |           v           v           v
    |      Process     Send       Update
    |       Data    Notifications Analytics
    |
    +---> Stores WiFi Network
```

**Flow:**
1. API receives WiFi QR creation request
2. Saves WiFi network to SQL Server database
3. Publishes event message to AWS SQS queue
4. Lambda function automatically triggered by SQS
5. Lambda processes event (notifications, analytics, logging)
6. Message deleted from queue after successful processing

## API Endpoints

### Authentication
- `POST /api/auth/login` - Login with username/password to get JWT token

### WiFi Networks
- `POST /api/wifi` - Create new WiFi network (sends event to SQS)
- `GET /api/wifi` - Get all WiFi networks
- `GET /api/wifi/{id}` - Get WiFi network by ID
- `GET /api/wifi/{id}/qr` - Download QR code PNG
- `POST /api/wifi/bulk-qr` - Download multiple QR codes as ZIP

## Tech Stack

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

## Getting Started

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

## Authentication

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

:warning: **For development only:**
- Username: `admin`
- Password: `password123`

**Important:** Change these in production!

## Example Usage

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

:sparkles: **When SQS is enabled:** A message is automatically sent to AWS SQS for asynchronous processing!

### Download QR Code

```http
GET /api/wifi/{id}/qr
Authorization: Bearer YOUR_TOKEN_HERE
```

Returns a PNG image that devices can scan to connect to the WiFi network.

## AWS Integration

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
- :email: Send email notifications
- :bar_chart: Update analytics databases
- :bell: Trigger other workflows
- :pencil: Log events to CloudWatch

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

## Validation Rules

### WiFi Networks
- SSID: Required, max 32 characters
- Password: 8-63 characters (required for encrypted networks)
- Encryption: WPA, WPA2, WPA3, WEP, or nopass
- Password required for encrypted networks
- Password must be empty for open networks

### Bulk Download
- Minimum: 1 network
- Maximum: 50 networks per request

## Project Structure

```
FastAPIHomeWifiQR/
?
??? Endpoints/
?   ??? Auth/
?   ?   ??? LoginEndpoint.cs
?   ?   ??? LoginRequest.cs
?   ?   ??? LoginResponse.cs
?   ?   ??? LoginRequestValidator.cs
?   ?
?   ??? Wifi/
?       ??? CreateWifiEndpoint.cs
?       ??? CreateWifiRequest.cs
?       ??? CreateWifiResponse.cs
?       ??? CreateWifiRequestValidator.cs
?       ??? GetAllWifiEndpoint.cs
?       ??? GetWifiByIdEndpoint.cs
?       ??? GetWifiByIdRequest.cs
?       ??? GetWifiByIdRequestValidator.cs
?       ??? DownloadQrCodeEndpoint.cs
?       ??? DownloadQrCodeRequest.cs
?       ??? DownloadQrCodeRequestValidator.cs
?       ??? DownloadBulkQrCodesEndpoint.cs
?       ??? DownloadBulkQrCodesRequest.cs
?       ??? DownloadBulkQrCodesRequestValidator.cs
?       ??? WifiNetworkResponse.cs
?
??? Services/
?   ??? WifiService.cs
?   ??? QrCodeService.cs
?   ??? AgileMapperAdapter.cs
?   ??? ISqsService.cs
?   ??? SqsService.cs
?
??? Models/
?   ??? WifiNetwork.cs
?   ??? WifiQrCreatedMessage.cs
?
??? Data/
?   ??? ApplicationDbContext.cs
?
??? Program.cs
??? appsettings.json
??? appsettings.Development.json
??? FastAPIHomeWifiQR.csproj
```

### Key Components

#### Endpoints
FastEndpoints-based request handlers organized by feature:
- **Auth/** - Authentication and JWT token generation
- **Wifi/** - WiFi network management and QR code operations

#### Services
Business logic and external integrations:
- **WifiService** - WiFi network operations
- **QrCodeService** - QR code generation
- **SqsService** - AWS SQS message publishing
- **AgileMapperAdapter** - Object mapping configuration

#### Models
Domain entities and data transfer objects:
- **WifiNetwork** - WiFi network entity (EF Core)
- **WifiQrCreatedMessage** - SQS message model

#### Data
Database context and configuration:
- **ApplicationDbContext** - EF Core database context
