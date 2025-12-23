# ?? Real-Time Logs Viewing Guide

## ?? Overview

This guide shows you all the ways to view logs in real-time for your WiFi QR Generator application.

---

## ?? Method 1: AWS CloudWatch Logs (Lambda)

### **Option A: AWS CLI - Live Tail (Best!)**

```powershell
# View Lambda logs in real-time (auto-updates)
aws logs tail /aws/lambda/WifiQrProcessor --follow

# View logs from last 30 minutes
aws logs tail /aws/lambda/WifiQrProcessor --since 30m --follow

# View logs from last hour
aws logs tail /aws/lambda/WifiQrProcessor --since 1h --follow

# Filter for errors only
aws logs tail /aws/lambda/WifiQrProcessor --follow --filter-pattern "ERROR"

# Filter for specific SSID
aws logs tail /aws/lambda/WifiQrProcessor --follow --filter-pattern "SSID: MyWiFi"
```

**What you'll see:**
```
2024-01-15T10:30:00.123Z START RequestId: abc123...
2024-01-15T10:30:00.234Z Processing 1 messages from SQS
2024-01-15T10:30:00.345Z Processing message: msg-abc123
2024-01-15T10:30:00.456Z WiFi QR Code Created - ID: 550e8400..., SSID: MyWiFi, Encryption: WPA2
2024-01-15T10:30:00.567Z Sending email notification for WiFi: MyWiFi
2024-01-15T10:30:00.678Z Updating analytics for WiFi ID: 550e8400...
2024-01-15T10:30:00.789Z Successfully processed message: msg-abc123
2024-01-15T10:30:00.890Z END RequestId: abc123...
```

---

### **Option B: AWS Console - Live Tail**

1. **Go to CloudWatch:**
   - https://console.aws.amazon.com/cloudwatch/

2. **Navigate to Log Groups:**
   - Click **"Logs"** ? **"Log groups"**
   - Find: `/aws/lambda/WifiQrProcessor`

3. **Start Live Tail:**
   - Click the **"Live tail"** button (top right)
   - Click **"Start"**
   - Logs appear in real-time!

4. **Filter Logs:**
   - Use the filter box to search: `ERROR`, `SSID`, etc.
   - Add time range filters

---

### **Option C: AWS Console - Log Streams**

1. **Go to CloudWatch:**
   - https://console.aws.amazon.com/cloudwatch/

2. **Select Log Group:**
   - Click on `/aws/lambda/WifiQrProcessor`

3. **View Latest Stream:**
   - Click on the **most recent log stream**
   - Enable auto-refresh: Click refresh icon ? Select **"10 seconds"**

---

## ?? Method 2: .NET API Logs (Your Application)

### **Option A: Console Output (Development)**

When running locally, logs appear in console:

```powershell
# Terminal 1: Start API and watch logs
cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
dotnet run

# You'll see real-time logs:
```

**Example output:**
```
2024-01-15 10:30:00.123 [INF] Starting WiFi QR Generator API...
2024-01-15 10:30:01.234 [INF] AWS SQS integration enabled
2024-01-15 10:30:02.345 [INF] API started successfully!
2024-01-15 10:30:05.456 [INF] Successfully sent message to SQS. MessageId: abc123, Type: WifiQrCreatedMessage
2024-01-15 10:30:10.567 [INF] QR code generated for SSID: MyWiFi
```

---

### **Option B: File Logs (Persistent + Real-Time)**

Logs are now saved to: `C:\Users\Akshitha\source\repos\FastApiHomeWifiQR\logs\`

#### **View logs in real-time using PowerShell:**

```powershell
# Method 1: Tail the log file
Get-Content -Path "logs\wifi-qr-$(Get-Date -Format 'yyyyMMdd').txt" -Wait -Tail 20

# Method 2: Follow the log file
Get-Content -Path "logs\wifi-qr-*.txt" -Wait
```

#### **View logs in real-time using `tail` (Git Bash/WSL):**

```bash
# Navigate to project directory
cd /c/Users/Akshitha/source/repos/FastApiHomeWifiQR

# Tail the log file
tail -f logs/wifi-qr-$(date +%Y%m%d).txt

# Or watch all log files
tail -f logs/*.txt
```

#### **View logs in Visual Studio:**

1. Open Solution Explorer
2. Right-click **"logs"** folder ? **"Show All Files"**
3. Double-click the log file
4. File updates automatically as new logs are written

---

### **Option C: Windows Terminal (Split Panes)**

Run API and watch logs simultaneously:

```powershell
# Pane 1: Run API
cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
dotnet run

# Pane 2 (Ctrl+Shift+Plus to split): Watch log file
Get-Content -Path "logs\wifi-qr-*.txt" -Wait -Tail 20
```

---

## ?? Method 3: Complete End-to-End Monitoring

### **Setup: 3 Terminal Windows**

#### **Terminal 1: API Logs**
```powershell
cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
dotnet run
```

#### **Terminal 2: Lambda Logs**
```powershell
aws logs tail /aws/lambda/WifiQrProcessor --follow
```

#### **Terminal 3: Test and Monitor**
```powershell
# Send test message
aws sqs send-message `
  --queue-url https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue `
  --message-body '{
    "wifiId": "550e8400-e29b-41d4-a716-446655440000",
    "ssid": "TestWiFi",
    "encryption": "WPA2",
    "hidden": false,
    "createdAt": "2024-01-15T10:30:00Z",
    "createdBy": "TestUser"
  }'

# Watch Terminal 2 for Lambda processing!
```

---

## ?? Method 4: Log File Analysis Tools

### **Option A: Using PowerShell**

```powershell
# Search for errors in today's log
Select-String -Path "logs\wifi-qr-*.txt" -Pattern "ERROR"

# Search for specific SSID
Select-String -Path "logs\wifi-qr-*.txt" -Pattern "MyWiFi"

# Count log entries by level
Get-Content logs\wifi-qr-*.txt | Select-String -Pattern "\[(INF|WRN|ERR|FTL)\]" | Group-Object

# Get last 50 lines
Get-Content logs\wifi-qr-*.txt | Select-Object -Last 50
```

---

### **Option B: Using Notepad++/VSCode**

1. **Open log file:**
   ```
   C:\Users\Akshitha\source\repos\FastApiHomeWifiQR\logs\wifi-qr-YYYYMMDD.txt
   ```

2. **In Notepad++:**
   - View ? Monitoring (tail -f) ? Enable

3. **In VS Code:**
   - Install extension: "Log File Highlighter"
   - File auto-refreshes when modified

---

## ?? Method 5: CloudWatch Insights (Advanced Queries)

### **Using AWS Console:**

1. **Go to CloudWatch:**
   - https://console.aws.amazon.com/cloudwatch/

2. **Navigate to Logs Insights:**
   - Click **"Logs"** ? **"Insights"**

3. **Select Log Group:**
   - Choose: `/aws/lambda/WifiQrProcessor`

4. **Run Queries:**

```sql
-- Get all WiFi QR codes created in last hour
fields @timestamp, @message
| filter @message like /WiFi QR Code Created/
| sort @timestamp desc
| limit 20

-- Count by SSID
fields @message
| parse @message /SSID: (?<ssid>[^,]+)/
| stats count() by ssid

-- Find errors
fields @timestamp, @message
| filter @message like /ERROR/
| sort @timestamp desc

-- Average processing time
fields @duration
| stats avg(@duration) as avgDuration
```

---

### **Using AWS CLI:**

```powershell
# Query logs
aws logs start-query `
  --log-group-name /aws/lambda/WifiQrProcessor `
  --start-time (Get-Date).AddHours(-1).ToUnixTimeMilliseconds() `
  --end-time (Get-Date).ToUnixTimeMilliseconds() `
  --query-string "fields @timestamp, @message | filter @message like /ERROR/ | sort @timestamp desc"
```

---

## ?? Method 6: Monitoring Dashboard

### **Create a Monitoring Dashboard:**

**PowerShell script - `monitor.ps1`:**

```powershell
# monitor.ps1
param(
    [string]$QueueUrl = "https://sqs.us-east-1.amazonaws.com/YOUR-ACCOUNT-ID/wifi-qr-queue"
)

Clear-Host
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  WiFi QR Generator - Live Monitoring" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Start background jobs
$apiJob = Start-Job -ScriptBlock {
    Get-Content -Path "logs\wifi-qr-*.txt" -Wait -Tail 5 |
    ForEach-Object { Write-Host "[API] $_" -ForegroundColor Green }
}

$lambdaJob = Start-Job -ScriptBlock {
    aws logs tail /aws/lambda/WifiQrProcessor --follow |
    ForEach-Object { Write-Host "[LAMBDA] $_" -ForegroundColor Yellow }
}

# Monitor in foreground
while ($true) {
    Clear-Host
    Write-Host "============================================" -ForegroundColor Cyan
    Write-Host "  WiFi QR Generator - Live Status" -ForegroundColor Cyan
    Write-Host "============================================" -ForegroundColor Cyan
    
    # SQS Queue Status
    $queueStats = aws sqs get-queue-attributes `
        --queue-url $QueueUrl `
        --attribute-names ApproximateNumberOfMessages,ApproximateNumberOfMessagesNotVisible `
        | ConvertFrom-Json
    
    Write-Host ""
    Write-Host "SQS Queue Status:" -ForegroundColor Cyan
    Write-Host "  Messages Available: $($queueStats.Attributes.ApproximateNumberOfMessages)" -ForegroundColor $(if ($queueStats.Attributes.ApproximateNumberOfMessages -eq 0) { "Green" } else { "Yellow" })
    Write-Host "  Messages In Flight: $($queueStats.Attributes.ApproximateNumberOfMessagesNotVisible)" -ForegroundColor $(if ($queueStats.Attributes.ApproximateNumberOfMessagesNotVisible -eq 0) { "Green" } else { "Yellow" })
    
    Write-Host ""
    Write-Host "Recent Logs:" -ForegroundColor Cyan
    Write-Host "(Watching logs in background...)" -ForegroundColor Gray
    
    # Receive job outputs
    Receive-Job -Job $apiJob
    Receive-Job -Job $lambdaJob
    
    Start-Sleep -Seconds 5
}
```

**Run:**
```powershell
.\monitor.ps1
```

---

## ?? Quick Commands Summary

```powershell
# ???????????????????????????????????????????????
#  Quick Commands for Real-Time Log Viewing
# ???????????????????????????????????????????????

# 1. Lambda Logs (AWS)
aws logs tail /aws/lambda/WifiQrProcessor --follow

# 2. API Console Logs (Local)
cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
dotnet run

# 3. API File Logs (Real-time)
Get-Content -Path "logs\wifi-qr-*.txt" -Wait -Tail 20

# 4. Search Logs
Select-String -Path "logs\wifi-qr-*.txt" -Pattern "ERROR"

# 5. Monitor Everything
# Terminal 1: dotnet run
# Terminal 2: aws logs tail /aws/lambda/WifiQrProcessor --follow
# Terminal 3: Get-Content logs\wifi-qr-*.txt -Wait
```

---

## ?? Log Locations

| Log Type | Location | Format |
|----------|----------|--------|
| **Lambda Logs** | CloudWatch `/aws/lambda/WifiQrProcessor` | AWS CloudWatch |
| **API Console** | Terminal output when running `dotnet run` | Console |
| **API File Logs** | `C:\Users\Akshitha\source\repos\FastApiHomeWifiQR\logs\wifi-qr-YYYYMMDD.txt` | Text file (rotates daily) |
| **SQS Metrics** | CloudWatch Metrics | AWS Console |

---

## ?? Recommended Setup for Development

### **Option 1: Split Terminal (Best for Development)**

Use Windows Terminal with split panes:

1. **Pane 1 (Left):** Run API
   ```powershell
   cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
   dotnet run
   ```

2. **Pane 2 (Top Right):** Watch Lambda logs
   ```powershell
   aws logs tail /aws/lambda/WifiQrProcessor --follow
   ```

3. **Pane 3 (Bottom Right):** Watch file logs
   ```powershell
   cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR
   Get-Content -Path "logs\wifi-qr-*.txt" -Wait -Tail 20
   ```

---

### **Option 2: Simple (Just Starting)**

```powershell
# Just watch Lambda logs
aws logs tail /aws/lambda/WifiQrProcessor --follow
```

This shows Lambda processing in real-time!

---

## ?? What You'll See

### **When Everything Works:**

**API Logs:**
```
2024-01-15 10:30:00.123 [INF] Successfully sent message to SQS. MessageId: abc123
```

**Lambda Logs (2-5 seconds later):**
```
Processing 1 messages from SQS
WiFi QR Code Created - ID: 550e8400..., SSID: MyWiFi, Encryption: WPA2
Sending email notification for WiFi: MyWiFi
Updating analytics for WiFi ID: 550e8400...
Successfully processed message: msg-abc123
```

**File Logs:**
```
2024-01-15 10:30:00.123 +00:00 [INF] Successfully sent message to SQS. MessageId: abc123, Type: WifiQrCreatedMessage
2024-01-15 10:30:00.234 +00:00 [INF] QR code generated for SSID: MyWiFi
```

---

## ?? Troubleshooting

### **No logs appearing in CloudWatch:**
```powershell
# Check if Lambda has been invoked
aws lambda get-function --function-name WifiQrProcessor

# Check event source mapping
aws lambda list-event-source-mappings --function-name WifiQrProcessor
```

### **File logs not created:**
```powershell
# Check if logs directory exists
ls logs/

# If not, create it
mkdir logs

# Restart API
dotnet run
```

### **Can't find log file:**
```powershell
# Find today's log file
Get-ChildItem -Path logs/ -Filter "wifi-qr-*.txt" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
```

---

## ?? Best Practice

**For active development:**
```powershell
# Keep this running in a terminal
aws logs tail /aws/lambda/WifiQrProcessor --follow
```

This gives you **instant feedback** when messages are processed! ??

---

## ?? Additional Resources

- **CloudWatch Logs Documentation:** https://docs.aws.amazon.com/AmazonCloudWatch/latest/logs/
- **Serilog Documentation:** https://serilog.net/
- **AWS CLI Logs Commands:** https://docs.aws.amazon.com/cli/latest/reference/logs/

---

**?? You now have real-time log viewing configured!**
