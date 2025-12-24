# :ship: AWS SQS + Lambda - Quick Reference

## :zap: Quick Setup (5 Steps)

### 1:ship: Configure AWS Credentials
```bash
aws configure
```

### 2:ship: Create SQS Queue
```bash
aws sqs create-queue --queue-name wifi-qr-queue
```

### 3:ship: Update appsettings.json
```json
{
  "AWS": {
    "Region": "us-east-1",
    "SQS": {
      "QueueUrl": "YOUR_QUEUE_URL_HERE",
      "EnableSqs": true
    }
  }
}
```

### 4:ship: Run Your API
```bash
dotnet run
```

### 5:ship: Create WiFi QR Code
API will automatically send message to SQS!

---

## :ship: Quick Commands

### **SQS Commands:**
```bash
# Create queue
aws sqs create-queue --queue-name wifi-qr-queue

# Get queue URL
aws sqs get-queue-url --queue-name wifi-qr-queue

# Send test message
aws sqs send-message \
  --queue-url YOUR_QUEUE_URL \
  --message-body '{"test": "message"}'

# Check queue status
aws sqs get-queue-attributes \
  --queue-url YOUR_QUEUE_URL \
  --attribute-names All

# Purge queue
aws sqs purge-queue --queue-url YOUR_QUEUE_URL
```

### **Lambda Commands:**
```bash
# Create Lambda function
dotnet new lambda.EmptyFunction -n WifiQrLambdaProcessor

# Build and package
dotnet lambda package -c Release

# Deploy
dotnet lambda deploy-function WifiQrProcessor

# Invoke test
aws lambda invoke \
  --function-name WifiQrProcessor \
  --payload '{"test": "data"}' \
  response.json
```

### **Monitoring Commands:**
```bash
# View Lambda logs
aws logs tail /aws/lambda/WifiQrProcessor --follow

# Check SQS metrics
aws cloudwatch get-metric-statistics \
  --namespace AWS/SQS \
  --metric-name ApproximateNumberOfMessagesVisible \
  --dimensions Name=QueueName,Value=wifi-qr-queue \
  --start-time $(date -u -d '1 hour ago' +%Y-%m-%dT%H:%M:%S) \
  --end-time $(date -u +%Y-%m-%dT%H:%M:%S) \
  --period 300 \
  --statistics Sum
```

---

## :ship: Testing Flow

### **Test End-to-End:**
```bash
# 1. Start API
dotnet run

# 2. Create WiFi QR (will send to SQS)
curl -X POST http://localhost:5014/api/wifi \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ssid": "TestNetwork",
    "password": "Test123456",
    "encryption": "WPA2",
    "hidden": false
  }'

# 3. Check SQS queue
aws sqs get-queue-attributes \
  --queue-url YOUR_QUEUE_URL \
  --attribute-names ApproximateNumberOfMessages

# 4. View Lambda logs
aws logs tail /aws/lambda/WifiQrProcessor --follow
```

---

## :ship: Message Format

### **Message Sent to SQS:**
```json
{
  "wifiId": "550e8400-e29b-41d4-a716-446655440000",
  "ssid": "MyWiFi",
  "encryption": "WPA2",
  "hidden": false,
  "createdAt": "2024-01-15T10:30:00Z",
  "createdBy": "admin"
}
```

---

## :ship: Key URLs

- **AWS Console SQS:** https://console.aws.amazon.com/sqs/
- **AWS Console Lambda:** https://console.aws.amazon.com/lambda/
- **AWS Console CloudWatch:** https://console.aws.amazon.com/cloudwatch/

---

## :ship: Configuration Toggle

### **Disable SQS (Development):**
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": false
    }
  }
}
```

### **Enable SQS (Production):**
```json
{
  "AWS": {
    "SQS": {
      "EnableSqs": true
    }
  }
}
```

---

## :ship: Quick Troubleshooting

| Problem | Solution |
|---------|----------|
| Messages not sent | Check `EnableSqs` = true in appsettings.json |
| AWS credentials error | Run `aws configure` |
| Queue URL not found | Update `QueueUrl` in appsettings.json |
| Lambda not triggered | Create event source mapping |
| Permission denied | Check IAM roles |

---

## :ship: Support Resources

- **Documentation:** See `AWS_SQS_LAMBDA_GUIDE.md`
- **AWS Support:** https://console.aws.amazon.com/support/
- **SDK Documentation:** https://docs.aws.amazon.com/sdk-for-net/

---

**Quick Start:** Configure AWS :zap: Create Queue :zap: Update settings :zap: Run! :ship:
