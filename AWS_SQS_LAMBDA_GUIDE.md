# ?? AWS SQS and Lambda Integration Guide

## ?? Overview

This guide explains how to set up Amazon SQS and AWS Lambda to process WiFi QR code creation events asynchronously.

---

## ??? Architecture

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

---

## ?? What Was Added

### **.NET API Changes**

1. **NuGet Packages:**
   - `AWSSDK.SQS` - Amazon SQS client
   - `AWSSDK.Extensions.NETCore.Setup` - AWS SDK integration

2. **New Files:**
   - `Models/WifiQrCreatedMessage.cs` - SQS message model
   - `Services/ISqsService.cs` - SQS service interface
   - `Services/SqsService.cs` - SQS service implementation

3. **Updated Files:**
   - `appsettings.json` - AWS configuration
   - `Program.cs` - AWS service registration
   - `Endpoints/Wifi/CreateWifiEndpoint.cs` - Send SQS messages

---

## ?? Setup Instructions

### **Step 1: Configure AWS Credentials**

#### Option A: AWS CLI (Recommended for Development)

```bash
# Install AWS CLI
# Download from: https://aws.amazon.com/cli/

# Configure credentials
aws configure

# Enter:
# AWS Access Key ID: YOUR_ACCESS_KEY
# AWS Secret Access Key: YOUR_SECRET_KEY
# Default region: us-east-1
# Default output format: json
```

#### Option B: Environment Variables

```powershell
# Windows PowerShell
$env:AWS_ACCESS_KEY_ID="YOUR_ACCESS_KEY"
$env:AWS_SECRET_ACCESS_KEY="YOUR_SECRET_KEY"
$env:AWS_REGION="us-east-1"
```

```bash
# Linux/Mac
export AWS_ACCESS_KEY_ID="YOUR_ACCESS_KEY"
export AWS_SECRET_ACCESS_KEY="YOUR_SECRET_KEY"
export AWS_REGION="us-east-1"
```

#### Option C: AWS Profile (Production)

Create `~/.aws/credentials`:
```ini
[default]
aws_access_key_id = YOUR_ACCESS_KEY
aws_secret_access_key = YOUR_SECRET_KEY
region = us-east-1
```

---

### **Step 2: Create SQS Queue**

#### Using AWS Console:

1. **Go to Amazon SQS:**
   - https://console.aws.amazon.com/sqs/

2. **Create Queue:**
   - Click **"Create queue"**
   - **Type:** Standard (or FIFO for ordered processing)
   - **Name:** `wifi-qr-queue` (or `wifi-qr-queue.fifo` for FIFO)
   
3. **Configuration:**
   - **Visibility timeout:** 30 seconds
   - **Message retention:** 4 days
   - **Receive message wait time:** 20 seconds (enable long polling)
   - **Maximum message size:** 256 KB

4. **Copy Queue URL:**
   ```
   https://sqs.us-east-1.amazonaws.com/123456789012/wifi-qr-queue
   ```

#### Using AWS CLI:

```bash
# Create Standard Queue
aws sqs create-queue \
  --queue-name wifi-qr-queue \
  --attributes VisibilityTimeout=30,MessageRetentionPeriod=345600

# Get Queue URL
aws sqs get-queue-url --queue-name wifi-qr-queue

# Create FIFO Queue (optional)
aws sqs create-queue \
  --queue-name wifi-qr-queue.fifo \
  --attributes FifoQueue=true,ContentBasedDeduplication=true
```

---

### **Step 3: Update appsettings.json**

Replace `YOUR-ACCOUNT-ID` with your AWS account ID:

```json
{
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

To find your AWS Account ID:
```bash
aws sts get-caller-identity --query Account --output text
```

---

### **Step 4: Create AWS Lambda Function**

#### Create Lambda Project:

```bash
# Install Lambda templates
dotnet new -i Amazon.Lambda.Templates

# Create new Lambda function
dotnet new lambda.EmptyFunction -n WifiQrLambdaProcessor

cd WifiQrLambdaProcessor
```

#### Lambda Function Code:

Create `Function.cs`:
```csharp
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;

// Assembly attribute to enable Lambda JSON serialization
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace WifiQrLambdaProcessor;

public class Function
{
    /// <summary>
    /// Processes messages from SQS queue
    /// </summary>
    public async Task FunctionHandler(SQSEvent sqsEvent, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processing {sqsEvent.Records.Count} messages from SQS");

        foreach (var record in sqsEvent.Records)
        {
            await ProcessMessageAsync(record, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        try
        {
            context.Logger.LogInformation($"Processing message: {message.MessageId}");

            // Deserialize the WiFi QR created message
            var wifiMessage = JsonSerializer.Deserialize<WifiQrCreatedMessage>(
                message.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (wifiMessage == null)
            {
                context.Logger.LogWarning($"Failed to deserialize message: {message.MessageId}");
                return;
            }

            // Process the message
            context.Logger.LogInformation(
                $"WiFi QR Code Created - ID: {wifiMessage.WifiId}, " +
                $"SSID: {wifiMessage.Ssid}, Encryption: {wifiMessage.Encryption}");

            // TODO: Add your processing logic here
            // Examples:
            // - Send email notification
            // - Update analytics database
            // - Trigger other workflows
            // - Store in data warehouse
            
            await SendEmailNotificationAsync(wifiMessage, context);
            await UpdateAnalyticsAsync(wifiMessage, context);

            context.Logger.LogInformation($"Successfully processed message: {message.MessageId}");
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error processing message {message.MessageId}: {ex.Message}");
            throw; // Throw to return message to queue
        }
    }

    private async Task SendEmailNotificationAsync(WifiQrCreatedMessage message, ILambdaContext context)
    {
        // TODO: Implement email notification
        context.Logger.LogInformation($"Sending email notification for SSID: {message.Ssid}");
        await Task.CompletedTask;
    }

    private async Task UpdateAnalyticsAsync(WifiQrCreatedMessage message, ILambdaContext context)
    {
        // TODO: Implement analytics update
        context.Logger.LogInformation($"Updating analytics for WiFi ID: {message.WifiId}");
        await Task.CompletedTask;
    }
}

/// <summary>
/// Model matching the message sent from .NET API
/// </summary>
public class WifiQrCreatedMessage
{
    public Guid WifiId { get; set; }
    public string Ssid { get; set; } = string.Empty;
    public string Encryption { get; set; } = string.Empty;
    public bool Hidden { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, string>? Metadata { get; set; }
}
```

---

### **Step 5: Deploy Lambda Function**

#### Using AWS CLI:

```bash
# Build and package
dotnet lambda package -c Release -o wifi-qr-lambda.zip

# Create IAM role for Lambda (one-time)
aws iam create-role \
  --role-name lambda-sqs-execution-role \
  --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [{
      "Effect": "Allow",
      "Principal": {"Service": "lambda.amazonaws.com"},
      "Action": "sts:AssumeRole"
    }]
  }'

# Attach policies
aws iam attach-role-policy \
  --role-name lambda-sqs-execution-role \
  --policy-arn arn:aws:iam::aws:policy/service-role/AWSLambdaSQSQueueExecutionRole

aws iam attach-role-policy \
  --role-name lambda-sqs-execution-role \
  --policy-arn arn:aws:iam::aws:policy/CloudWatchLogsFullAccess

# Deploy Lambda
aws lambda create-function \
  --function-name WifiQrProcessor \
  --runtime dotnet8 \
  --role arn:aws:iam::YOUR-ACCOUNT-ID:role/lambda-sqs-execution-role \
  --handler WifiQrLambdaProcessor::WifiQrLambdaProcessor.Function::FunctionHandler \
  --zip-file fileb://wifi-qr-lambda.zip \
  --timeout 30 \
  --memory-size 512
```

#### Using AWS Console:

1. **Go to AWS Lambda:**
   - https://console.aws.amazon.com/lambda/

2. **Create Function:**
   - Click **"Create function"**
   - Choose **"Author from scratch"**
   - **Function name:** WifiQrProcessor
   - **Runtime:** .NET 8
   - **Architecture:** x86_64

3. **Upload Code:**
   - Build: `dotnet lambda package`
   - Upload the generated ZIP file

4. **Configure:**
   - **Handler:** `WifiQrLambdaProcessor::WifiQrLambdaProcessor.Function::FunctionHandler`
   - **Timeout:** 30 seconds
   - **Memory:** 512 MB

---

### **Step 6: Connect SQS to Lambda**

#### Using AWS Console:

1. **Open Lambda Function**

2. **Add Trigger:**
   - Click **"Add trigger"**
   - Select **"SQS"**
   - Choose your queue: `wifi-qr-queue`
   - **Batch size:** 10
   - **Batch window:** 0 seconds
   - Enable **"Report batch item failures"**

3. **Save**

#### Using AWS CLI:

```bash
# Get Queue ARN
QUEUE_ARN=$(aws sqs get-queue-attributes \
  --queue-url https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue \
  --attribute-names QueueArn \
  --query 'Attributes.QueueArn' \
  --output text)

# Create event source mapping
aws lambda create-event-source-mapping \
  --function-name WifiQrProcessor \
  --event-source-arn $QUEUE_ARN \
  --batch-size 10 \
  --enabled
```

---

## ?? Testing

### **Test from .NET API:**

```powershell
# Start your API
dotnet run

# Create a WiFi QR code (will send message to SQS)
curl -X POST http://localhost:5014/api/wifi \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ssid": "TestWiFi",
    "password": "TestPassword123",
    "encryption": "WPA2",
    "hidden": false
  }'
```

### **Monitor Lambda Execution:**

```bash
# View CloudWatch Logs
aws logs tail /aws/lambda/WifiQrProcessor --follow

# Check SQS queue
aws sqs get-queue-attributes \
  --queue-url https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue \
  --attribute-names ApproximateNumberOfMessages
```

### **Manual SQS Test:**

```bash
# Send test message directly to SQS
aws sqs send-message \
  --queue-url https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue \
  --message-body '{
    "wifiId": "123e4567-e89b-12d3-a456-426614174000",
    "ssid": "TestNetwork",
    "encryption": "WPA2",
    "hidden": false,
    "createdAt": "2024-01-15T10:30:00Z",
    "createdBy": "System"
  }'
```

---

## ?? Monitoring & Debugging

### **CloudWatch Metrics:**

Monitor these metrics in AWS Console:
- **SQS:** `ApproximateNumberOfMessagesVisible`
- **Lambda:** `Invocations`, `Errors`, `Duration`
- **Lambda:** `ConcurrentExecutions`

### **CloudWatch Logs:**

```bash
# View recent logs
aws logs tail /aws/lambda/WifiQrProcessor --since 1h --follow

# Search logs
aws logs filter-log-events \
  --log-group-name /aws/lambda/WifiQrProcessor \
  --filter-pattern "ERROR"
```

### **.NET API Logs:**

Check application logs for SQS send operations:
```
Successfully sent message to SQS. MessageId: xxx, Type: WifiQrCreatedMessage
```

---

## ?? IAM Permissions

### **Lambda Execution Role:**

Required permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "sqs:ReceiveMessage",
        "sqs:DeleteMessage",
        "sqs:GetQueueAttributes"
      ],
      "Resource": "arn:aws:sqs:us-east-1:YOUR-ACCOUNT-ID:wifi-qr-queue"
    },
    {
      "Effect": "Allow",
      "Action": [
        "logs:CreateLogGroup",
        "logs:CreateLogStream",
        "logs:PutLogEvents"
      ],
      "Resource": "*"
    }
  ]
}
```

### **.NET API (EC2/ECS):**

Required permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "sqs:SendMessage",
        "sqs:GetQueueUrl"
      ],
      "Resource": "arn:aws:sqs:us-east-1:YOUR-ACCOUNT-ID:wifi-qr-queue"
    }
  ]
}
```

---

## ?? Configuration Options

### **appsettings.json Settings:**

```json
{
  "AWS": {
    "Profile": "default",        // AWS credentials profile
    "Region": "us-east-1",       // AWS region
    "SQS": {
      "QueueUrl": "https://...", // SQS queue URL
      "EnableSqs": true          // Enable/disable SQS
    }
  }
}
```

### **Environment-Specific Configuration:**

**appsettings.Development.json:**
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": false  // Disable in development
    }
  }
}
```

**appsettings.Production.json:**
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": true  // Enable in production
    }
  }
}
```

---

## ?? Deployment

### **Deploy .NET API:**

```bash
# Publish application
dotnet publish -c Release

# Deploy to:
# - Azure App Service
# - AWS Elastic Beanstalk
# - AWS ECS/Fargate
# - Docker container
```

### **Deploy Lambda Function:**

```bash
# Update existing function
dotnet lambda deploy-function WifiQrProcessor -c Release
```

---

## ?? Scaling Considerations

### **SQS Queue:**
- **Standard Queue:** Unlimited throughput
- **FIFO Queue:** 300 TPS (3000 with batching)

### **Lambda Function:**
- **Concurrent executions:** 1000 (default)
- **Batch size:** 1-10 messages
- **Reserve capacity** for high traffic

### **.NET API:**
- **Connection pooling:** Enabled by default
- **Retry policy:** Implement for transient failures

---

## ?? Cost Estimation

### **Monthly Costs (approx):**

| Service | Usage | Cost |
|---------|-------|------|
| SQS | 1M requests | $0.40 |
| Lambda | 1M invocations @ 512MB, 1s | $8.35 |
| CloudWatch Logs | 1GB | $0.50 |
| **Total** | | **~$9.25/month** |

*Free tier covers first 1M Lambda requests and 1M SQS requests*

---

## ?? Troubleshooting

### **Messages not appearing in queue:**
1. Check `EnableSqs` setting in appsettings.json
2. Verify AWS credentials
3. Check API logs for SQS errors
4. Validate queue URL

### **Lambda not triggered:**
1. Verify event source mapping exists
2. Check Lambda execution role permissions
3. Verify queue has messages

### **Lambda errors:**
1. Check CloudWatch Logs
2. Verify message format matches model
3. Check timeout settings

---

## ?? Additional Resources

- **AWS SQS Documentation:** https://docs.aws.amazon.com/sqs/
- **AWS Lambda .NET:** https://docs.aws.amazon.com/lambda/latest/dg/csharp-handler.html
- **AWS SDK for .NET:** https://docs.aws.amazon.com/sdk-for-net/
- **Lambda PowerTools:** https://awslabs.github.io/aws-lambda-powertools-dotnet/

---

**?? Integration Complete!** Your WiFi QR Generator now sends events to SQS for asynchronous processing!
