# :ship: Kubernetes Quick Reference

## :zap: Quick Deploy

```bash
# 1. Build and push image
docker build -t yourusername/wifi-qr-api:latest .
docker push yourusername/wifi-qr-api:latest

# 2. Update image in k8s/api-deployment.yaml

# 3. Deploy everything
kubectl apply -f k8s/

# 4. Check status
kubectl get pods -n wifi-qr

# 5. Run migrations
kubectl apply -f k8s/migration-job.yaml

# 6. Access API
kubectl port-forward -n wifi-qr svc/wifi-qr-api-service 5014:80
```

---

## :wrench: Essential Commands

### **Pods**
```bash
# List pods
kubectl get pods -n wifi-qr

# Describe pod
kubectl describe pod <pod-name> -n wifi-qr

# Logs
kubectl logs -n wifi-qr -l app=wifi-qr-api -f

# Execute command
kubectl exec -n wifi-qr <pod-name> -- dotnet --version

# Shell access
kubectl exec -it -n wifi-qr <pod-name> -- /bin/bash
```

### **Deployments**
```bash
# List deployments
kubectl get deployments -n wifi-qr

# Scale
kubectl scale deployment wifi-qr-api -n wifi-qr --replicas=5

# Restart
kubectl rollout restart deployment wifi-qr-api -n wifi-qr

# Status
kubectl rollout status deployment wifi-qr-api -n wifi-qr

# History
kubectl rollout history deployment wifi-qr-api -n wifi-qr

# Rollback
kubectl rollout undo deployment wifi-qr-api -n wifi-qr
```

### **Services**
```bash
# List services
kubectl get svc -n wifi-qr

# Describe service
kubectl describe svc wifi-qr-api-service -n wifi-qr

# Port forward
kubectl port-forward -n wifi-qr svc/wifi-qr-api-service 5014:80
```

### **ConfigMaps & Secrets**
```bash
# View configmap
kubectl get configmap wifi-qr-api-config -n wifi-qr -o yaml

# Edit configmap
kubectl edit configmap wifi-qr-api-config -n wifi-qr

# View secrets (base64 encoded)
kubectl get secret wifi-qr-api-secrets -n wifi-qr -o yaml

# Decode secret
kubectl get secret wifi-qr-api-secrets -n wifi-qr -o jsonpath='{.data.jwt-signing-key}' | base64 --decode
```

---

## :mag: Debugging

### **Check Everything**
```bash
# Cluster info
kubectl cluster-info

# All resources
kubectl get all -n wifi-qr

# Events
kubectl get events -n wifi-qr --sort-by='.lastTimestamp'

# Describe problematic resource
kubectl describe pod <pod-name> -n wifi-qr
```

### **Database Connectivity**
```bash
# Test from API pod
POD=$(kubectl get pods -n wifi-qr -l app=wifi-qr-api -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n wifi-qr $POD -- ping sqlserver-service

# Access SQL Server
kubectl exec -it -n wifi-qr -l app=sqlserver -- /bin/bash
```

### **Network Testing**
```bash
# Create test pod
kubectl run test -n wifi-qr --rm -it --image=busybox -- sh

# Test DNS
nslookup wifi-qr-api-service
nslookup sqlserver-service

# Test HTTP
wget -O- http://wifi-qr-api-service/health
```

---

## :chart_with_upwards_trend: Monitoring

### **Resource Usage**
```bash
# Node metrics
kubectl top nodes

# Pod metrics
kubectl top pods -n wifi-qr

# Watch HPA
kubectl get hpa -n wifi-qr -w
```

### **Logs**
```bash
# All API pods
kubectl logs -n wifi-qr -l app=wifi-qr-api --tail=100

# Follow logs
kubectl logs -n wifi-qr -l app=wifi-qr-api -f

# Previous container
kubectl logs -n wifi-qr <pod-name> --previous

# Multiple containers
kubectl logs -n wifi-qr <pod-name> -c <container-name>
```

---

## :hammer_and_wrench: Common Tasks

### **Update Image**
```bash
# Build new image
docker build -t yourusername/wifi-qr-api:v1.1 .
docker push yourusername/wifi-qr-api:v1.1

# Update deployment
kubectl set image deployment/wifi-qr-api -n wifi-qr \
  wifi-qr-api=yourusername/wifi-qr-api:v1.1

# Or edit deployment
kubectl edit deployment wifi-qr-api -n wifi-qr
```

### **Run Migrations**
```bash
# Using job
kubectl apply -f k8s/migration-job.yaml
kubectl logs -n wifi-qr job/wifi-qr-db-migration

# Manual
POD=$(kubectl get pods -n wifi-qr -l app=wifi-qr-api -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n wifi-qr $POD -- dotnet ef database update
```

### **Scale Application**
```bash
# Manual scale
kubectl scale deployment wifi-qr-api -n wifi-qr --replicas=5

# Auto-scale
kubectl autoscale deployment wifi-qr-api -n wifi-qr \
  --min=2 --max=10 --cpu-percent=70
```

---

## :broom: Cleanup

```bash
# Delete namespace (everything)
kubectl delete namespace wifi-qr

# Delete specific resources
kubectl delete deployment wifi-qr-api -n wifi-qr
kubectl delete statefulset sqlserver -n wifi-qr
kubectl delete svc --all -n wifi-qr

# Delete PVCs
kubectl delete pvc -n wifi-qr --all
```

---

## :rocket: Cluster Management

### **Minikube**
```bash
# Start
minikube start

# Stop
minikube stop

# Delete
minikube delete

# Dashboard
minikube dashboard

# Service access
minikube service wifi-qr-api-service -n wifi-qr
```

### **Context Switching**
```bash
# List contexts
kubectl config get-contexts

# Use context
kubectl config use-context docker-desktop
kubectl config use-context minikube
kubectl config use-context arn:aws:eks:...

# Current context
kubectl config current-context
```

---

## :link: Quick URLs

### **Local Access**
```bash
# Port forward
kubectl port-forward -n wifi-qr svc/wifi-qr-api-service 5014:80

# URLs
# Swagger: http://localhost:5014/swagger
# Health: http://localhost:5014/health
```

### **Minikube**
```bash
# Get URL
minikube service wifi-qr-api-service -n wifi-qr --url

# Open in browser
minikube service wifi-qr-api-service -n wifi-qr
```

### **LoadBalancer**
```bash
# Get external IP
kubectl get svc -n wifi-qr wifi-qr-api-service

# Access
# http://<EXTERNAL-IP>/swagger
```

---

## :books: Full Documentation

See [KUBERNETES_DEPLOYMENT.md](KUBERNETES_DEPLOYMENT.md) for complete guide.

---

**Quick Tips:**
- Use `-w` flag to watch resources: `kubectl get pods -n wifi-qr -w`
- Use `--dry-run=client -o yaml` to generate YAML without applying
- Use `kubectl explain <resource>` to learn about resource fields
- Use `k9s` terminal UI for easier cluster management
