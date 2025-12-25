# :whale: Docker Deployment Guide

## :books: Overview

This guide explains how to containerize and deploy the WiFi QR Generator API using Docker and Docker Compose.

---

## :package: What's Included

### **Docker Files:**
- **Dockerfile** - Multi-stage build for optimized .NET 8 API
- **docker-compose.yml** - Development environment with SQL Server
- **docker-compose.prod.yml** - Production-ready configuration
- **.dockerignore** - Excludes unnecessary files from Docker build
- **.env.example** - Environment variables template

---

## :rocket: Quick Start

### **Option 1: Development with Docker Compose (Recommended)**

```bash
# 1. Copy environment variables
cp .env.example .env

# 2. Edit .env with your settings
notepad .env  # Windows
# or
nano .env     # Linux/Mac

# 3. Start all services
docker-compose up -d

# 4. View logs
docker-compose logs -f wifi-qr-api

# 5. Access the API
# Open: http://localhost:5014/swagger
```

### **Option 2: Docker Only (API without Database)**

```bash
# Build image
docker build -t wifi-qr-api:latest .

# Run container
docker run -d \
  -p 5014:8080 \
  -e ConnectionStrings__DefaultConnection="Your-Connection-String" \
  -e JwtSettings__SigningKey="your-secret-key" \
  --name wifi-qr-api \
  wifi-qr-api:latest

# View logs
docker logs -f wifi-qr-api
```

---

## :building_construction: Docker Architecture

### **Multi-Stage Dockerfile:**

```
Stage 1: Build (SDK)
    |
    v
Stage 2: Publish
    |
    v
Stage 3: Runtime (ASP.NET)
```

**Benefits:**
- ? Smaller final image (~200MB vs ~800MB)
- ? Faster deployment
- ? Better security (runtime only, no SDK)
- ? Optimized for production

---

## :wrench: Prerequisites

### **Required:**
- **Docker Desktop** (Windows/Mac) or **Docker Engine** (Linux)
  - Download: https://www.docker.com/products/docker-desktop/
- **Docker Compose** (included with Docker Desktop)

### **Optional:**
- **Docker Hub Account** (for pushing images)
- **AWS Account** (for SQS/Lambda integration)

---

## :hammer_and_wrench: Installation Steps

### **Step 1: Install Docker**

#### Windows:
```powershell
# Download Docker Desktop from:
# https://www.docker.com/products/docker-desktop/

# Verify installation
docker --version
docker-compose --version
```

#### Linux (Ubuntu):
```bash
# Update package index
sudo apt-get update

# Install Docker
sudo apt-get install docker.io docker-compose

# Start Docker service
sudo systemctl start docker
sudo systemctl enable docker

# Add user to docker group (optional)
sudo usermod -aG docker $USER
newgrp docker

# Verify installation
docker --version
docker-compose --version
```

#### macOS:
```bash
# Download Docker Desktop from:
# https://www.docker.com/products/docker-desktop/

# Or use Homebrew
brew install --cask docker

# Verify installation
docker --version
docker-compose --version
```

---

### **Step 2: Configure Environment Variables**

```bash
# Copy example file
cp .env.example .env
```

**Edit `.env` file:**

```env
# Database Configuration
SQL_PASSWORD=YourStrong@Passw0rd

# JWT Configuration
JWT_SIGNING_KEY=your-secret-signing-key-at-least-32-characters-long

# AWS Configuration (Optional)
AWS_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key
AWS_SQS_QUEUE_URL=https://sqs.us-east-1.amazonaws.com/123456789012/wifi-qr-queue
ENABLE_SQS=false
```

---

### **Step 3: Build and Run**

#### **Development Mode:**

```bash
# Start all services (API + SQL Server)
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

#### **Production Mode:**

```bash
# Start with production config
docker-compose -f docker-compose.prod.yml up -d

# View logs
docker-compose -f docker-compose.prod.yml logs -f

# Stop services
docker-compose -f docker-compose.prod.yml down
```

---

## :gear: Configuration Details

### **Dockerfile Breakdown:**

```dockerfile
# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FastAPIHomeWifiQR.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "FastAPIHomeWifiQR.dll"]
```

### **Key Features:**
- ? Multi-stage build (optimized size)
- ? Non-root user (security)
- ? Health checks
- ? Environment-specific configs
- ? Volume persistence

---

## :test_tube: Testing

### **1. Check Container Status:**

```bash
# List running containers
docker ps

# Check API health
curl http://localhost:5014/health

# Or visit in browser
open http://localhost:5014/swagger
```

### **2. View Logs:**

```bash
# API logs
docker-compose logs -f wifi-qr-api

# SQL Server logs
docker-compose logs -f sqlserver

# All logs
docker-compose logs -f
```

### **3. Execute Commands in Container:**

```bash
# Open shell in API container
docker exec -it wifi-qr-api /bin/bash

# Run EF migrations
docker exec -it wifi-qr-api \
  dotnet ef database update

# Check environment variables
docker exec wifi-qr-api printenv
```

---

## :package: Database Management

### **Run Migrations:**

```bash
# Option 1: From host machine
dotnet ef database update

# Option 2: Inside container
docker exec -it wifi-qr-api \
  dotnet ef database update

# Option 3: Docker Compose exec
docker-compose exec wifi-qr-api \
  dotnet ef database update
```

### **Backup Database:**

```bash
# Backup SQL Server data
docker exec wifi-qr-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourPassword" \
  -Q "BACKUP DATABASE HomeWifiQRDb TO DISK='/var/opt/mssql/backup/HomeWifiQRDb.bak'"

# Copy backup to host
docker cp wifi-qr-sqlserver:/var/opt/mssql/backup/HomeWifiQRDb.bak ./backup/
```

### **Restore Database:**

```bash
# Copy backup to container
docker cp ./backup/HomeWifiQRDb.bak wifi-qr-sqlserver:/var/opt/mssql/backup/

# Restore
docker exec wifi-qr-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourPassword" \
  -Q "RESTORE DATABASE HomeWifiQRDb FROM DISK='/var/opt/mssql/backup/HomeWifiQRDb.bak'"
```

---

## :cloud: Deployment Options

### **1. Docker Hub (Public Registry)**

```bash
# Login to Docker Hub
docker login

# Tag image
docker tag wifi-qr-api:latest yourusername/wifi-qr-api:latest

# Push to Docker Hub
docker push yourusername/wifi-qr-api:latest

# Pull on another machine
docker pull yourusername/wifi-qr-api:latest
```

### **2. AWS ECR (Private Registry)**

```bash
# Login to ECR
aws ecr get-login-password --region us-east-1 | \
  docker login --username AWS --password-stdin \
  123456789012.dkr.ecr.us-east-1.amazonaws.com

# Tag image
docker tag wifi-qr-api:latest \
  123456789012.dkr.ecr.us-east-1.amazonaws.com/wifi-qr-api:latest

# Push to ECR
docker push \
  123456789012.dkr.ecr.us-east-1.amazonaws.com/wifi-qr-api:latest
```

### **3. Azure Container Registry**

```bash
# Login to ACR
az acr login --name yourregistry

# Tag image
docker tag wifi-qr-api:latest \
  yourregistry.azurecr.io/wifi-qr-api:latest

# Push to ACR
docker push yourregistry.azurecr.io/wifi-qr-api:latest
```

---

## :ship: Production Deployment

### **Using docker-compose.prod.yml:**

```bash
# Create production .env file
cp .env.example .env.production

# Edit with production values
nano .env.production

# Deploy
docker-compose -f docker-compose.prod.yml --env-file .env.production up -d

# Scale API instances
docker-compose -f docker-compose.prod.yml up -d --scale wifi-qr-api=3
```

### **Using Docker Swarm:**

```bash
# Initialize swarm
docker swarm init

# Deploy stack
docker stack deploy -c docker-compose.prod.yml wifi-qr-stack

# List services
docker service ls

# View logs
docker service logs -f wifi-qr-stack_wifi-qr-api

# Scale service
docker service scale wifi-qr-stack_wifi-qr-api=3

# Remove stack
docker stack rm wifi-qr-stack
```

### **Using Kubernetes:**

```bash
# Generate Kubernetes manifests
kompose convert -f docker-compose.yml

# Apply to cluster
kubectl apply -f .

# Check status
kubectl get pods
kubectl get services

# View logs
kubectl logs -f deployment/wifi-qr-api
```

---

## :bar_chart: Monitoring

### **Container Stats:**

```bash
# Real-time stats
docker stats

# Container resource usage
docker stats wifi-qr-api

# Inspect container
docker inspect wifi-qr-api
```

### **Health Checks:**

```bash
# Check health status
docker ps

# Manual health check
curl http://localhost:5014/health

# Container health history
docker inspect wifi-qr-api | grep -A 10 Health
```

### **Logs:**

```bash
# Follow logs
docker-compose logs -f wifi-qr-api

# Last 100 lines
docker-compose logs --tail=100 wifi-qr-api

# Since timestamp
docker-compose logs --since 2024-01-15T10:00:00 wifi-qr-api

# Export logs
docker-compose logs wifi-qr-api > api-logs.txt
```

---

## :bug: Troubleshooting

### **Common Issues:**

#### **1. Container won't start:**

```bash
# Check logs
docker-compose logs wifi-qr-api

# Inspect container
docker inspect wifi-qr-api

# Check port conflicts
netstat -ano | findstr :5014  # Windows
lsof -i :5014                 # Linux/Mac
```

#### **2. Database connection errors:**

```bash
# Check SQL Server is running
docker ps | grep sqlserver

# Test database connection
docker exec wifi-qr-sqlserver \
  /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourPassword" \
  -Q "SELECT 1"

# Check connection string
docker exec wifi-qr-api printenv | grep ConnectionStrings
```

#### **3. Cannot access Swagger:**

```bash
# Check if API is listening
docker exec wifi-qr-api netstat -tuln

# Check firewall
# Windows: Check Windows Firewall
# Linux: sudo ufw status

# Test from inside container
docker exec wifi-qr-api curl http://localhost:8080/health
```

#### **4. High memory usage:**

```bash
# Check container limits
docker stats

# Update resource limits in docker-compose.yml:
deploy:
  resources:
    limits:
      memory: 512M
    reservations:
      memory: 256M
```

### **Debugging Commands:**

```bash
# Enter container shell
docker exec -it wifi-qr-api /bin/bash

# Check environment variables
docker exec wifi-qr-api printenv

# Check running processes
docker exec wifi-qr-api ps aux

# Check disk usage
docker exec wifi-qr-api df -h

# Network diagnostics
docker exec wifi-qr-api ping sqlserver
```

---

## :lock: Security Best Practices

### **1. Use Secrets for Sensitive Data:**

```bash
# Create Docker secret
echo "YourSecretKey" | docker secret create jwt_signing_key -

# Use in docker-compose.yml:
secrets:
  jwt_signing_key:
    external: true

services:
  wifi-qr-api:
    secrets:
      - jwt_signing_key
```

### **2. Run as Non-Root User:**

```dockerfile
# Already implemented in Dockerfile
RUN useradd -m -u 1000 appuser
USER appuser
```

### **3. Scan for Vulnerabilities:**

```bash
# Scan image
docker scan wifi-qr-api:latest

# Trivy scan
trivy image wifi-qr-api:latest
```

### **4. Network Isolation:**

```yaml
# Use custom networks
networks:
  frontend:
  backend:

services:
  wifi-qr-api:
    networks:
      - frontend
      - backend
  sqlserver:
    networks:
      - backend  # Not exposed to frontend
```

---

## :moneybag: Performance Optimization

### **1. Build Optimization:**

```dockerfile
# Use BuildKit for faster builds
# Set: DOCKER_BUILDKIT=1

# Multi-stage caching
# Separate restore and build steps
```

### **2. Runtime Optimization:**

```bash
# Set memory limits
docker run -m 512M wifi-qr-api

# Set CPU limits
docker run --cpus="1.5" wifi-qr-api

# Use read-only root filesystem
docker run --read-only wifi-qr-api
```

### **3. Image Size Reduction:**

```dockerfile
# Use alpine images (if compatible)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

# Remove unnecessary files
RUN rm -rf /var/cache/apk/*
```

---

## :chart_with_upwards_trend: Scaling

### **Horizontal Scaling:**

```bash
# Scale with Docker Compose
docker-compose up -d --scale wifi-qr-api=3

# Scale with Docker Swarm
docker service scale wifi-qr-stack_wifi-qr-api=5

# Scale with Kubernetes
kubectl scale deployment wifi-qr-api --replicas=5
```

### **Load Balancing:**

```yaml
# Use nginx as load balancer
services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
    depends_on:
      - wifi-qr-api
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf
```

---

## :books: Additional Resources

- **Docker Documentation:** https://docs.docker.com/
- **Docker Compose:** https://docs.docker.com/compose/
- **ASP.NET Core Docker:** https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/
- **SQL Server Docker:** https://docs.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker

---

## :white_check_mark: Deployment Checklist

- [ ] Install Docker and Docker Compose
- [ ] Create `.env` file from `.env.example`
- [ ] Update environment variables
- [ ] Build Docker image
- [ ] Test locally with `docker-compose up`
- [ ] Run database migrations
- [ ] Test API endpoints
- [ ] Configure health checks
- [ ] Set up monitoring
- [ ] Configure logging
- [ ] Implement backup strategy
- [ ] Deploy to production
- [ ] Set up CI/CD pipeline

---

**:tada: Containerization Complete!** Your WiFi QR Generator is now Docker-ready!

For AWS Lambda deployment, see [AWS_SQS_LAMBDA_GUIDE.md](AWS_SQS_LAMBDA_GUIDE.md)
