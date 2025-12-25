# :whale: Docker Quick Reference

## :zap: Quick Commands

### **Start Services:**
```bash
docker-compose up -d
```

### **Stop Services:**
```bash
docker-compose down
```

### **View Logs:**
```bash
docker-compose logs -f wifi-qr-api
```

### **Rebuild:**
```bash
docker-compose up -d --build
```

### **Access Swagger:**
```
http://localhost:5014/swagger
```

---

## :rocket: One-Line Deployments

### **Development:**
```bash
cp .env.example .env && docker-compose up -d
```

### **Production:**
```bash
docker-compose -f docker-compose.prod.yml up -d
```

### **Rebuild Everything:**
```bash
docker-compose down -v && docker-compose up -d --build
```

---

## :wrench: Common Tasks

### **Run Migrations:**
```bash
docker-compose exec wifi-qr-api dotnet ef database update
```

### **View Database:**
```bash
docker exec -it wifi-qr-sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourPassword"
```

### **Shell Access:**
```bash
docker exec -it wifi-qr-api /bin/bash
```

### **Check Health:**
```bash
curl http://localhost:5014/health
```

---

## :package: Docker Images

### **Build:**
```bash
docker build -t wifi-qr-api:latest .
```

### **Tag:**
```bash
docker tag wifi-qr-api:latest yourusername/wifi-qr-api:v1.0
```

### **Push:**
```bash
docker push yourusername/wifi-qr-api:v1.0
```

### **Pull:**
```bash
docker pull yourusername/wifi-qr-api:v1.0
```

---

## :bug: Debugging

### **Container Status:**
```bash
docker ps -a
```

### **Container Stats:**
```bash
docker stats wifi-qr-api
```

### **Inspect:**
```bash
docker inspect wifi-qr-api
```

### **Logs (Last 100 lines):**
```bash
docker logs --tail 100 wifi-qr-api
```

---

## :broom: Cleanup

### **Stop and Remove Containers:**
```bash
docker-compose down
```

### **Remove Volumes:**
```bash
docker-compose down -v
```

### **Remove Images:**
```bash
docker rmi wifi-qr-api:latest
```

### **Prune System:**
```bash
docker system prune -a
```

---

## :gear: Environment Variables

```env
SQL_PASSWORD=YourStrong@Passw0rd
JWT_SIGNING_KEY=your-secret-key-32-chars
AWS_REGION=us-east-1
ENABLE_SQS=false
```

---

## :link: Quick Links

- **Swagger:** http://localhost:5014/swagger
- **Health:** http://localhost:5014/health
- **SQL Server:** localhost:1433

---

**Full Documentation:** [DOCKER_DEPLOYMENT.md](DOCKER_DEPLOYMENT.md)
