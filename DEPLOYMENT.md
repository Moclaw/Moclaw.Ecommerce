# Ecommerce Microservices CI/CD with Minikube

This project implements a comprehensive CI/CD pipeline for the Ecommerce microservices platform using Minikube with dedicated node assignment for each service.

## 🏗️ Architecture

- **Ecom.Core**: Core business logic service
- **Ecom.Users**: User management service
- **Node Separation**: Each service runs on dedicated nodes for better isolation

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- Minikube
- kubectl
- .NET 9.0 SDK

### Setup Cluster
```bash
# Make scripts executable
chmod +x *.sh

# Setup Minikube cluster with multiple nodes
./setup-minikube.sh

# Configure node affinity for service separation
./setup-node-affinity.sh

# Deploy services
./deploy.sh
```

### Test Deployment
```bash
# Run comprehensive tests
./test-cluster.sh

# Access services
minikube service list --profile=ecommerce-cluster
```

## 📁 Project Structure

```
├── k8s/                          # Kubernetes manifests
│   ├── namespace.yml             # Namespace and basic config
│   ├── storage-secrets.yml       # PV, PVC, and secrets
│   ├── network-policies.yml      # Network security policies
│   ├── hpa.yml                   # Horizontal Pod Autoscaler
│   ├── monitoring.yml            # Prometheus monitoring
│   ├── core-service/             # Core service manifests
│   │   └── deployment.yml
│   └── users-service/            # Users service manifests
│       └── deployment.yml
├── .github/workflows/            # CI/CD pipeline
│   └── cicd.yml
├── Ecom.Core/                    # Core service source
├── Ecom.Users/                   # Users service source
└── scripts/                      # Deployment scripts
    ├── setup-minikube.sh         # Cluster setup
    ├── setup-node-affinity.sh    # Node configuration
    ├── deploy.sh                 # Service deployment
    ├── test-cluster.sh           # Testing script
    └── cleanup.sh                # Cleanup script
```

## 🔧 Configuration

### Node Assignment Strategy

**Two-Node Cluster (Recommended):**
- Node 1: Ecom.Core service
- Node 2: Ecom.Users service

**Single-Node Cluster:**
- Both services with priority scheduling

**Multi-Node Cluster (3+):**
- Node 1: Ecom.Core service
- Node 2: Ecom.Users service
- Other nodes: System workloads

### Service Configuration

Each service includes:
- **Resource Limits**: CPU and memory constraints
- **Health Checks**: Liveness and readiness probes
- **Auto-scaling**: HPA based on CPU/memory usage
- **Network Policies**: Service-to-service communication rules
- **Persistent Storage**: Shared volume for data exchange

## 🛠️ Available Scripts

### Setup Scripts
```bash
./setup-minikube.sh          # Initialize Minikube cluster
./setup-node-affinity.sh     # Configure node labels and affinity
```

### Deployment Scripts
```bash
./deploy.sh                  # Deploy all services
./test-cluster.sh           # Run health checks and tests
```

### Cleanup Scripts
```bash
./cleanup.sh partial        # Remove deployments only
./cleanup.sh full          # Delete entire cluster
./cleanup.sh images        # Clean Docker images only
```

## 🌐 Access Services

### Local Development
```bash
# Port forwarding
kubectl port-forward -n ecommerce service/ecom-core-service 8080:80
kubectl port-forward -n ecommerce service/ecom-users-service 8081:80

# Access APIs
curl http://localhost:8080/health
curl http://localhost:8081/health
```

### Ingress (Production-like)
```bash
# Get Minikube IP
minikube ip --profile=ecommerce-cluster

# Add to /etc/hosts
<MINIKUBE_IP> core.ecommerce.local
<MINIKUBE_IP> users.ecommerce.local

# Access via domain names
curl http://core.ecommerce.local/health
curl http://users.ecommerce.local/health
```

## 📊 Monitoring

### Cluster Dashboard
```bash
minikube dashboard --profile=ecommerce-cluster
```

### Resource Monitoring
```bash
# Node resources
kubectl top nodes

# Pod resources
kubectl top pods -n ecommerce

# HPA status
kubectl get hpa -n ecommerce
```

### Prometheus Metrics
```bash
# Port forward Prometheus
kubectl port-forward -n ecommerce service/prometheus-service 9090:9090

# Access Prometheus UI
open http://localhost:9090
```

## 🔒 Security Features

- **Network Policies**: Restrict inter-service communication
- **Secrets Management**: Encrypted storage for sensitive data
- **Resource Quotas**: Prevent resource exhaustion
- **Node Isolation**: Service separation for better security

## 🚢 CI/CD Pipeline

The GitHub Actions workflow includes:

1. **Code Quality**: Build and test .NET projects
2. **Container Build**: Docker image creation
3. **Cluster Setup**: Multi-node Minikube initialization
4. **Node Configuration**: Service-specific node assignment
5. **Deployment**: Rolling deployment with health checks
6. **Testing**: Comprehensive endpoint and integration tests
7. **Monitoring**: Resource usage and performance metrics

### Pipeline Triggers
- Push to `main` or `develop` branches
- Pull requests to `main`

### Pipeline Stages
```yaml
jobs:
  build-and-deploy:
    - Checkout code
    - Setup .NET environment
    - Run unit tests
    - Start Minikube cluster
    - Configure node affinity
    - Build Docker images
    - Deploy to Kubernetes
    - Run health checks
    - Generate deployment report
```

## 🔧 Customization

### Scaling Configuration
Edit `k8s/hpa.yml` to modify auto-scaling parameters:
```yaml
minReplicas: 2
maxReplicas: 10
targetCPUUtilizationPercentage: 70
```

### Resource Limits
Modify `k8s/*/deployment.yml` for resource allocation:
```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "200m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

### Network Policies
Update `k8s/network-policies.yml` for custom security rules.

## 🐛 Troubleshooting

### Common Issues

**Pods Pending:**
```bash
kubectl describe pod <pod-name> -n ecommerce
kubectl get events -n ecommerce --sort-by='.lastTimestamp'
```

**Service Not Accessible:**
```bash
kubectl get svc -n ecommerce
kubectl port-forward -n ecommerce service/<service-name> <local-port>:80
```

**Node Affinity Issues:**
```bash
kubectl get nodes --show-labels
./setup-node-affinity.sh
```

### Logs and Debugging
```bash
# Service logs
kubectl logs -f deployment/ecom-core-api -n ecommerce
kubectl logs -f deployment/ecom-users-api -n ecommerce

# Cluster events
kubectl get events -n ecommerce --sort-by='.lastTimestamp'

# Node status
kubectl describe nodes
```

## 📚 Additional Resources

- [Minikube Documentation](https://minikube.sigs.k8s.io/docs/)
- [Kubernetes Best Practices](https://kubernetes.io/docs/concepts/)
- [.NET on Kubernetes](https://docs.microsoft.com/en-us/dotnet/architecture/containerized-lifecycle/)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make changes and test locally
4. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE.txt file for details.
