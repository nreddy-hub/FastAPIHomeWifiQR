# :ship: Kubernetes Deployment Guide

## :books: Overview

This guide explains how to deploy the WiFi QR Generator API to Kubernetes clusters (Minikube, EKS, AKS, GKE).

---

## :package: Architecture

```
Kubernetes Cluster
    |
    +-- Namespace: wifi-qr
         |
         +-- Deployment: wifi-qr-api (3 replicas)
         |    |
         |    +-- Pod 1 (.NET 8 API)
         |    +-- Pod 2 (.NET 8 API)
         |    +-- Pod 3 (.NET 8 API)
         |
         +-- StatefulSet: sqlserver (1 replica)
         |    |
         |    +-- Pod (SQL Server 2022)
         |    +-- PersistentVolume (10Gi)
         |
         +-- Service: wifi-qr-api-service (LoadBalancer)
         +-- Service: sqlserver-service (ClusterIP)
         +-- HorizontalPodAutoscaler (2-10 replicas)
         +-- Ingress (HTTPS with cert-manager)
```

---

## :wrench: Prerequisites

### **Required:**
- **kubectl** - Kubernetes CLI
- **Docker** - For building images
- **Kubernetes Cluster** - One of:
  - Minikube (local)
  - Docker Desktop Kubernetes (local)
  - AWS EKS (cloud)
  - Azure AKS (cloud)
  - Google GKE (cloud)

### **Optional:**
- **Helm** - Package manager for Kubernetes
- **k9s** - Terminal UI for Kubernetes
- **Lens** - Kubernetes IDE

---

## :hammer_and_wrench: Installation

### **Step 1: Install kubectl**

#### Windows (PowerShell):
```powershell
# Download kubectl
curl.exe -LO "https://dl.k8s.io/release/v1.28.0/bin/windows/amd64/kubectl.exe"

# Move to PATH location
Move-Item .\kubectl.exe C:\Windows\System32\

# Verify
kubectl version --client
```

#### Linux:
```bash
curl -LO "https://dl.k8s.io/release/$(curl -L -s https://dl.k8s.io/release/stable.txt)/bin/linux/amd64/kubectl"
chmod +x kubectl
sudo mv kubectl /usr/local/bin/
kubectl version --client
```

#### macOS:
```bash
brew install kubectl
kubectl version --client
```

### **Step 2: Setup Kubernetes Cluster**

#### **Option A: Minikube (Local Development)**

```bash
# Install Minikube
# Windows: choco install minikube
# Linux: curl -LO https://storage.googleapis.com/minikube/releases/latest/minikube-linux-amd64
# macOS: brew install minikube

# Start Minikube
minikube start --memory=4096 --cpus=2

# Verify
kubectl cluster-info
kubectl get nodes
```

#### **Option B: Docker Desktop Kubernetes (Local)**

```powershell
# Enable Kubernetes in Docker Desktop:
# 1. Open Docker Desktop
# 2. Go to Settings > Kubernetes
# 3. Check "Enable Kubernetes"
# 4. Click "Apply & Restart"

# Verify
kubectl config get-contexts
kubectl config use-context docker-desktop
kubectl cluster-info
```

#### **Option C: AWS EKS (Production)**

```bash
# Install eksctl
# Windows: choco install eksctl
# Linux/macOS: curl --silent --location "https://github.com/weaveworks/eksctl/releases/latest/download/eksctl_$(uname -s)_amd64.tar.gz" | tar xz -C /tmp && sudo mv /tmp/eksctl /usr/local/bin

# Create EKS cluster
eksctl create cluster \
  --name wifi-qr-cluster \
  --region us-east-1 \
  --nodegroup-name standard-workers \
  --node-type t3.medium \
  --nodes 3 \
  --nodes-min 2 \
  --nodes-max 5

# Configure kubectl
aws eks update-kubeconfig --region us-east-1 --name wifi-qr-cluster

# Verify
kubectl get nodes
```

#### **Option D: Azure AKS (Production)**

```bash
# Install Azure CLI
# Windows: choco install azure-cli
# Linux: curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash
# macOS: brew install azure-cli

# Login
az login

# Create resource group
az group create --name wifi-qr-rg --location eastus

# Create AKS cluster
az aks create \
  --resource-group wifi-qr-rg \
  --name wifi-qr-cluster \
  --node-count 3 \
  --enable-addons monitoring \
  --generate-ssh-keys

# Get credentials
az aks get-credentials --resource-group wifi-qr-rg --name wifi-qr-cluster

# Verify
kubectl get nodes
```

#### **Option E: Google GKE (Production)**

```bash
# Install gcloud CLI
# https://cloud.google.com/sdk/docs/install

# Initialize
gcloud init

# Create GKE cluster
gcloud container clusters create wifi-qr-cluster \
  --num-nodes=3 \
  --zone=us-central1-a \
  --machine-type=e2-medium

# Get credentials
gcloud container clusters get-credentials wifi-qr-cluster --zone=us-central1-a

# Verify
kubectl get nodes
```

---

## :rocket: Deployment Steps

### **Step 1: Build and Push Docker Image**

```bash
# Navigate to project directory
cd C:\Users\Akshitha\source\repos\FastApiHomeWifiQR

# Build Docker image
docker build -t wifi-qr-api:latest .

# Tag image for registry
# Docker Hub
docker tag wifi-qr-api:latest yourusername/wifi-qr-api:latest

# AWS ECR
docker tag wifi-qr-api:latest 123456789012.dkr.ecr.us-east-1.amazonaws.com/wifi-qr-api:latest

# Azure ACR
docker tag wifi-qr-api:latest yourregistry.azurecr.io/wifi-qr-api:latest

# Google GCR
docker tag wifi-qr-api:latest gcr.io/your-project/wifi-qr-api:latest

# Push to registry
docker push yourusername/wifi-qr-api:latest
```

### **Step 2: Update Kubernetes Manifests**

Update image references in `k8s/api-deployment.yaml`:

```yaml
# Change this line:
image: nreddy/wifi-qr-api:latest

# To your image:
image: yourusername/wifi-qr-api:latest
```

### **Step 3: Create Secrets**

```bash
# Create base64 encoded secrets
# JWT Signing Key
echo -n 'your-secret-key-at-least-32-characters-long' | base64

# Database Password
echo -n 'YourStrong@Passw0rd' | base64

# Database Connection String
echo -n 'Server=sqlserver-service;Database=HomeWifiQRDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;' | base64

# Update k8s/secrets.yaml with encoded values
```

### **Step 4: Deploy to Kubernetes**

```bash
# Apply all manifests
kubectl apply -f k8s/

# Or apply individually in order:
kubectl apply -f k8s/namespace.yaml
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/secrets.yaml
kubectl apply -f k8s/sqlserver-statefulset.yaml
kubectl apply -f k8s/sqlserver-service.yaml
kubectl apply -f k8s/api-deployment.yaml
kubectl apply -f k8s/api-service.yaml
kubectl apply -f k8s/hpa.yaml
kubectl apply -f k8s/ingress.yaml
```

### **Step 5: Wait for Pods to be Ready**

```bash
# Watch pod status
kubectl get pods -n wifi-qr -w

# Check specific pod logs
kubectl logs -n wifi-qr -l app=wifi-qr-api --tail=100

# Check SQL Server logs
kubectl logs -n wifi-qr -l app=sqlserver --tail=100
```

### **Step 6: Run Database Migrations**

```bash
# Option 1: Using Job
kubectl apply -f k8s/migration-job.yaml

# Check job status
kubectl get jobs -n wifi-qr
kubectl logs -n wifi-qr job/wifi-qr-db-migration

# Option 2: Manual execution
POD_NAME=$(kubectl get pods -n wifi-qr -l app=wifi-qr-api -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n wifi-qr $POD_NAME -- dotnet ef database update
```

### **Step 7: Access the API**

```bash
# Get service external IP (LoadBalancer)
kubectl get svc -n wifi-qr wifi-qr-api-service

# For Minikube
minikube service wifi-qr-api-service -n wifi-qr

# For port-forward (testing)
kubectl port-forward -n wifi-qr svc/wifi-qr-api-service 5014:80

# Access Swagger
# http://localhost:5014/swagger (port-forward)
# http://<EXTERNAL-IP>/swagger (LoadBalancer)
```

---

## :gear: Configuration Management

### **Update ConfigMap**

```bash
# Edit configmap
kubectl edit configmap wifi-qr-api-config -n wifi-qr

# Or apply updated file
kubectl apply -f k8s/configmap.yaml

# Restart pods to pick up changes
kubectl rollout restart deployment wifi-qr-api -n wifi-qr
```

### **Update Secrets**

```bash
# Create new secret values
kubectl create secret generic wifi-qr-api-secrets \
  --from-literal=jwt-signing-key='your-new-key' \
  --from-literal=db-connection-string='Server=sqlserver-service;...' \
  -n wifi-qr \
  --dry-run=client -o yaml | kubectl apply -f -

# Restart pods
kubectl rollout restart deployment wifi-qr-api -n wifi-qr
```

---

## :chart_with_upwards_trend: Scaling

### **Manual Scaling**

```bash
# Scale deployment
kubectl scale deployment wifi-qr-api -n wifi-qr --replicas=5

# Check status
kubectl get pods -n wifi-qr -l app=wifi-qr-api
```

### **Autoscaling (HPA)**

```bash
# HPA is automatically applied
kubectl get hpa -n wifi-qr

# View HPA details
kubectl describe hpa wifi-qr-api-hpa -n wifi-qr

# Watch HPA in action
kubectl get hpa -n wifi-qr -w
```

---

## :bar_chart: Monitoring

### **Pod Status**

```bash
# List all pods
kubectl get pods -n wifi-qr

# Detailed pod info
kubectl describe pod <pod-name> -n wifi-qr

# Pod resource usage
kubectl top pods -n wifi-qr
```

### **Logs**

```bash
# View logs
kubectl logs -n wifi-qr -l app=wifi-qr-api --tail=100

# Follow logs
kubectl logs -n wifi-qr -l app=wifi-qr-api -f

# Previous container logs
kubectl logs -n wifi-qr <pod-name> --previous

# Multiple pods
kubectl logs -n wifi-qr -l app=wifi-qr-api --all-containers=true
```

### **Events**

```bash
# View cluster events
kubectl get events -n wifi-qr --sort-by='.lastTimestamp'

# Watch events
kubectl get events -n wifi-qr -w
```

### **Metrics**

```bash
# Install metrics-server (if not already installed)
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

# View metrics
kubectl top nodes
kubectl top pods -n wifi-qr
```

---

## :bug: Troubleshooting

### **Pods not starting**

```bash
# Check pod status
kubectl get pods -n wifi-qr

# Describe problematic pod
kubectl describe pod <pod-name> -n wifi-qr

# Check logs
kubectl logs <pod-name> -n wifi-qr

# Check events
kubectl get events -n wifi-qr --sort-by='.lastTimestamp'
```

### **Database connection issues**

```bash
# Check SQL Server pod
kubectl get pods -n wifi-qr -l app=sqlserver

# Test connectivity
POD=$(kubectl get pods -n wifi-qr -l app=wifi-qr-api -o jsonpath='{.items[0].metadata.name}')
kubectl exec -n wifi-qr $POD -- ping sqlserver-service

# Check service endpoints
kubectl get endpoints -n wifi-qr sqlserver-service
```

### **Service not accessible**

```bash
# Check service
kubectl get svc -n wifi-qr

# Describe service
kubectl describe svc wifi-qr-api-service -n wifi-qr

# Check endpoints
kubectl get endpoints -n wifi-qr wifi-qr-api-service

# Test from inside cluster
kubectl run test-pod --rm -it --image=busybox -n wifi-qr -- wget -O- http://wifi-qr-api-service/health
```

### **Image pull errors**

```bash
# Check image pull secrets
kubectl get secrets -n wifi-qr

# Create image pull secret (if needed)
kubectl create secret docker-registry regcred \
  --docker-server=<registry> \
  --docker-username=<username> \
  --docker-password=<password> \
  -n wifi-qr

# Update deployment to use secret
# Add to api-deployment.yaml:
# imagePullSecrets:
# - name: regcred
```

---

## :broom: Cleanup

### **Delete Everything**

```bash
# Delete namespace (removes all resources)
kubectl delete namespace wifi-qr

# Or delete individually
kubectl delete -f k8s/
```

### **Delete Persistent Volumes**

```bash
# List PVCs
kubectl get pvc -n wifi-qr

# Delete PVC
kubectl delete pvc -n wifi-qr --all

# List PVs
kubectl get pv

# Delete PV (if needed)
kubectl delete pv <pv-name>
```

---

## :lock: Security Best Practices

### **1. Use RBAC**

```yaml
# Create service account
apiVersion: v1
kind: ServiceAccount
metadata:
  name: wifi-qr-api-sa
  namespace: wifi-qr

---
# Create role
apiVersion: rbac.authorization.k8s.io/v1
kind: Role
metadata:
  name: wifi-qr-api-role
  namespace: wifi-qr
rules:
- apiGroups: [""]
  resources: ["configmaps", "secrets"]
  verbs: ["get", "list"]

---
# Bind role to service account
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: wifi-qr-api-rolebinding
  namespace: wifi-qr
subjects:
- kind: ServiceAccount
  name: wifi-qr-api-sa
roleRef:
  kind: Role
  name: wifi-qr-api-role
  apiGroup: rbac.authorization.k8s.io
```

### **2. Network Policies**

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: api-network-policy
  namespace: wifi-qr
spec:
  podSelector:
    matchLabels:
      app: wifi-qr-api
  policyTypes:
  - Ingress
  - Egress
  ingress:
  - from:
    - namespaceSelector:
        matchLabels:
          name: ingress-nginx
    ports:
    - protocol: TCP
      port: 8080
  egress:
  - to:
    - podSelector:
        matchLabels:
          app: sqlserver
    ports:
    - protocol: TCP
      port: 1433
```

### **3. Pod Security Standards**

```yaml
apiVersion: v1
kind: Namespace
metadata:
  name: wifi-qr
  labels:
    pod-security.kubernetes.io/enforce: restricted
    pod-security.kubernetes.io/audit: restricted
    pod-security.kubernetes.io/warn: restricted
```

---

## :moneybag: Cost Optimization

### **Resource Requests/Limits**

```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

### **Node Affinity**

```yaml
affinity:
  nodeAffinity:
    preferredDuringSchedulingIgnoredDuringExecution:
    - weight: 1
      preference:
        matchExpressions:
        - key: node.kubernetes.io/instance-type
          operator: In
          values:
          - t3.medium
```

### **Cluster Autoscaler**

```bash
# AWS EKS
eksctl create cluster --asg-access --external-dns-access

# Azure AKS
az aks update \
  --resource-group wifi-qr-rg \
  --name wifi-qr-cluster \
  --enable-cluster-autoscaler \
  --min-count 2 \
  --max-count 5
```

---

## :books: Additional Resources

- **Kubernetes Documentation:** https://kubernetes.io/docs/
- **kubectl Cheat Sheet:** https://kubernetes.io/docs/reference/kubectl/cheatsheet/
- **Kubernetes Patterns:** https://k8spatterns.io/
- **Helm Charts:** https://helm.sh/

---

## :white_check_mark: Deployment Checklist

- [ ] Install kubectl
- [ ] Setup Kubernetes cluster
- [ ] Build and push Docker image
- [ ] Create/update secrets (base64 encoded)
- [ ] Update image references in manifests
- [ ] Apply namespace
- [ ] Apply configmap and secrets
- [ ] Deploy SQL Server StatefulSet
- [ ] Deploy API Deployment
- [ ] Apply Services
- [ ] Run database migrations
- [ ] Configure HPA
- [ ] Setup Ingress (optional)
- [ ] Configure monitoring
- [ ] Test API endpoints
- [ ] Setup backup strategy

---

**:tada: Kubernetes Deployment Complete!**

Your WiFi QR Generator API is now running on Kubernetes with:
- ? High availability (3 replicas)
- ? Auto-scaling (HPA)
- ? Load balancing
- ? Persistent database storage
- ? Health checks
- ? Rolling updates

For CI/CD integration, see [CI/CD with Kubernetes](#cicd-integration) section.
