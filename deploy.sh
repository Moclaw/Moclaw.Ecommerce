#!/bin/bash

# Deployment script for Ecommerce microservices
set -e

# Configuration
PROFILE="ecommerce-cluster"
NAMESPACE="ecommerce"

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to build and deploy a service
deploy_service() {
    local SERVICE_NAME=$1
    local DOCKERFILE_PATH=$2
    local MANIFESTS_PATH=$3
    
    print_status "Deploying $SERVICE_NAME..."
    
    # Build Docker image
    eval $(minikube docker-env --profile=$PROFILE)
    docker build -t $SERVICE_NAME:latest -f $DOCKERFILE_PATH .
    
    # Apply manifests
    kubectl apply -f $MANIFESTS_PATH
    
    # Wait for deployment
    DEPLOYMENT_NAME=$(basename $SERVICE_NAME)
    kubectl wait --for=condition=available --timeout=300s deployment/$DEPLOYMENT_NAME -n $NAMESPACE
    
    print_status "$SERVICE_NAME deployed successfully"
}

# Check if Minikube is running
if ! minikube status --profile=$PROFILE &> /dev/null; then
    print_error "Minikube cluster '$PROFILE' is not running. Please run ./setup-minikube.sh first."
    exit 1
fi

print_status "Starting deployment to Minikube cluster..."

# Ensure namespace exists
kubectl apply -f k8s/namespace.yml

# Deploy services
deploy_service "ecom-core-api" "Ecom.Core/src/Ecom.Core.API/Dockerfile" "k8s/core-service/"
deploy_service "ecom-users-api" "Ecom.Users/src/Ecom.Users.API/Dockerfile" "k8s/users-service/"

print_status "All services deployed successfully!"

# Show deployment status
echo ""
echo "📊 Deployment Status:"
echo "===================="
kubectl get deployments -n $NAMESPACE

echo ""
echo "🚀 Pods:"
echo "========"
kubectl get pods -n $NAMESPACE -o wide

echo ""
echo "🔌 Services:"
echo "============"
kubectl get services -n $NAMESPACE

echo ""
echo "🌐 Access Information:"
echo "======================"
MINIKUBE_IP=$(minikube ip --profile=$PROFILE)
echo "Minikube IP: $MINIKUBE_IP"
echo ""
echo "Add these lines to your /etc/hosts file:"
echo "$MINIKUBE_IP core.ecommerce.local"
echo "$MINIKUBE_IP users.ecommerce.local"
echo ""
echo "Then access:"
echo "- Core API: http://core.ecommerce.local"
echo "- Users API: http://users.ecommerce.local"
