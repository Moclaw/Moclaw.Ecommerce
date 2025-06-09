#!/bin/bash

# Ecommerce Minikube Setup Script
# This script sets up a multi-node Minikube cluster for Ecommerce microservices

set -e

echo "🚀 Setting up Minikube for Ecommerce project..."

# Configuration
CLUSTER_NAME="ecommerce-cluster"
NODES=2
K8S_VERSION="v1.28.0"
MEMORY="4096"
CPUS="2"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Minikube is installed
if ! command -v minikube &> /dev/null; then
    print_error "Minikube is not installed. Please install Minikube first."
    exit 1
fi

# Check if kubectl is installed
if ! command -v kubectl &> /dev/null; then
    print_error "kubectl is not installed. Please install kubectl first."
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    print_error "Docker is not running. Please start Docker first."
    exit 1
fi

print_status "Starting Minikube cluster with $NODES nodes..."

# Delete existing cluster if it exists
if minikube status --profile=$CLUSTER_NAME &> /dev/null; then
    print_warning "Existing cluster found. Deleting..."
    minikube delete --profile=$CLUSTER_NAME
fi

# Start Minikube with multiple nodes
minikube start \
    --profile=$CLUSTER_NAME \
    --nodes=$NODES \
    --kubernetes-version=$K8S_VERSION \
    --memory=$MEMORY \
    --cpus=$CPUS \
    --driver=docker \
    --addons=registry,ingress,metrics-server

print_status "Waiting for cluster to be ready..."
kubectl wait --for=condition=Ready nodes --all --timeout=300s

print_status "Labeling nodes for service deployment..."

# Get node names
NODES_ARRAY=($(kubectl get nodes -o jsonpath='{.items[*].metadata.name}'))

# Label nodes for specific services
if [ ${#NODES_ARRAY[@]} -ge 2 ]; then
    kubectl label nodes ${NODES_ARRAY[0]} service=core --overwrite
    kubectl label nodes ${NODES_ARRAY[1]} service=users --overwrite
    print_status "Labeled ${NODES_ARRAY[0]} for Core service"
    print_status "Labeled ${NODES_ARRAY[1]} for Users service"
else
    kubectl label nodes ${NODES_ARRAY[0]} service=core,users --overwrite
    print_warning "Single node detected. Both services will run on ${NODES_ARRAY[0]}"
fi

print_status "Creating namespace..."
kubectl apply -f k8s/namespace.yml

print_status "Building Docker images..."

# Configure Docker environment to use Minikube's Docker daemon
eval $(minikube docker-env --profile=$CLUSTER_NAME)

# Build images
docker build -t ecom-core-api:latest -f Ecom.Core/src/Ecom.Core.API/Dockerfile .
docker build -t ecom-users-api:latest -f Ecom.Users/src/Ecom.Users.API/Dockerfile .

print_status "Deploying services..."

# Deploy Core service
kubectl apply -f k8s/core-service/
kubectl wait --for=condition=available --timeout=300s deployment/ecom-core-api -n ecommerce

# Deploy Users service
kubectl apply -f k8s/users-service/
kubectl wait --for=condition=available --timeout=300s deployment/ecom-users-api -n ecommerce

print_status "Cluster setup complete!"

echo ""
echo "📊 Cluster Information:"
echo "======================"
kubectl cluster-info --context=$CLUSTER_NAME

echo ""
echo "🏷️  Node Labels:"
echo "==============="
kubectl get nodes --show-labels

echo ""
echo "📦 Deployments:"
echo "==============="
kubectl get deployments -n ecommerce

echo ""
echo "🔌 Services:"
echo "============"
kubectl get services -n ecommerce

echo ""
echo "🌐 Ingress:"
echo "==========="
kubectl get ingress -n ecommerce

echo ""
echo "🚀 Pods Distribution:"
echo "===================="
kubectl get pods -n ecommerce -o wide

echo ""
echo "✅ Setup completed successfully!"
echo ""
echo "🔗 Access URLs:"
echo "  Core API: http://core.ecommerce.local"
echo "  Users API: http://users.ecommerce.local"
echo ""
echo "💡 To access services locally, add these to your /etc/hosts:"
echo "  $(minikube ip --profile=$CLUSTER_NAME) core.ecommerce.local"
echo "  $(minikube ip --profile=$CLUSTER_NAME) users.ecommerce.local"
echo ""
echo "🛠️  Useful commands:"
echo "  minikube dashboard --profile=$CLUSTER_NAME"
echo "  kubectl get pods -n ecommerce -o wide"
echo "  minikube service list --profile=$CLUSTER_NAME"
