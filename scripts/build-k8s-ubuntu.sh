#!/bin/bash

# Build and Deploy Ecommerce Microservices to Kubernetes on Ubuntu
# Usage: ./build-k8s-ubuntu.sh [registry] [tag] [namespace]

set -e

# Default values
REGISTRY=${1:-"localhost:5000"}
TAG=${2:-"latest"}
NAMESPACE=${3:-"ecommerce"}
PROJECT_ROOT=$(dirname "$(dirname "$(realpath "$0")")")

echo "================================================"
echo "Building Ecommerce Microservices for Kubernetes"
echo "================================================"
echo "Registry: $REGISTRY"
echo "Tag: $TAG"
echo "Namespace: $NAMESPACE"
echo "Project Root: $PROJECT_ROOT"
echo ""

cd "$PROJECT_ROOT"

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check prerequisites
echo "Checking prerequisites..."
if ! command_exists docker; then
    echo "Error: Docker is not installed"
    exit 1
fi

if ! command_exists kubectl; then
    echo "Error: kubectl is not installed"
    exit 1
fi

if ! command_exists dotnet; then
    echo "Error: .NET SDK is not installed"
    exit 1
fi

echo "✓ All prerequisites are installed"
echo ""

# Clean previous builds
echo "Cleaning previous builds..."
docker system prune -f >/dev/null 2>&1 || true
echo "✓ Docker system cleaned"
echo ""

# Build Docker images
echo "Building Docker images..."

# Build Users API
echo "Building ecom-users-api..."
docker build \
    -t "${REGISTRY}/ecom-users-api:${TAG}" \
    -f "Ecom.Users/src/Ecom.Users.API/Dockerfile" \
    . || { echo "Failed to build Users API"; exit 1; }
echo "✓ Users API built successfully"

# Build Core API
echo "Building ecom-core-api..."
docker build \
    -t "${REGISTRY}/ecom-core-api:${TAG}" \
    -f "Ecom.Core/src/Ecom.Core.API/Dockerfile" \
    . || { echo "Failed to build Core API"; exit 1; }
echo "✓ Core API built successfully"
echo ""

# Push images to registry (skip if localhost)
if [ "$REGISTRY" != "localhost:5000" ]; then
    echo "Pushing images to registry..."
    docker push "${REGISTRY}/ecom-users-api:${TAG}" || { echo "Failed to push Users API"; exit 1; }
    docker push "${REGISTRY}/ecom-core-api:${TAG}" || { echo "Failed to push Core API"; exit 1; }
    echo "✓ Images pushed successfully"
else
    echo "Skipping push for localhost registry"
fi
echo ""

# Update Kubernetes manifests
echo "Updating Kubernetes manifests..."
if [ -f "Kubernetes/ecom-users-api.yaml" ]; then
    sed -i "s|image: .*ecom-users-api:.*|image: ${REGISTRY}/ecom-users-api:${TAG}|g" Kubernetes/ecom-users-api.yaml
    echo "✓ Updated Users API manifest"
fi

if [ -f "Kubernetes/ecom-core-api.yaml" ]; then
    sed -i "s|image: .*ecom-core-api:.*|image: ${REGISTRY}/ecom-core-api:${TAG}|g" Kubernetes/ecom-core-api.yaml
    echo "✓ Updated Core API manifest"
fi
echo ""

# Deploy to Kubernetes
echo "Deploying to Kubernetes..."

# Create namespace if it doesn't exist
kubectl create namespace "$NAMESPACE" --dry-run=client -o yaml | kubectl apply -f - || true
echo "✓ Namespace '$NAMESPACE' ready"

# Apply Kubernetes manifests
if [ -f "Kubernetes/namespace.yaml" ]; then
    kubectl apply -f Kubernetes/namespace.yaml
fi

if [ -f "Kubernetes/ecom-users-api.yaml" ]; then
    kubectl apply -f Kubernetes/ecom-users-api.yaml -n "$NAMESPACE"
    echo "✓ Users API deployed"
fi

if [ -f "Kubernetes/ecom-core-api.yaml" ]; then
    kubectl apply -f Kubernetes/ecom-core-api.yaml -n "$NAMESPACE"
    echo "✓ Core API deployed"
fi

if [ -f "Kubernetes/ingress.yaml" ]; then
    kubectl apply -f Kubernetes/ingress.yaml -n "$NAMESPACE"
    echo "✓ Ingress deployed"
fi

if [ -f "Kubernetes/configmap.yaml" ]; then
    kubectl apply -f Kubernetes/configmap.yaml -n "$NAMESPACE"
    echo "✓ ConfigMap applied"
fi

if [ -f "Kubernetes/secrets.yaml" ]; then
    kubectl apply -f Kubernetes/secrets.yaml -n "$NAMESPACE"
    echo "✓ Secrets applied"
fi
echo ""

# Wait for deployments to be ready
echo "Waiting for deployments to be ready..."
kubectl wait --for=condition=available --timeout=300s deployment/ecom-users-api -n "$NAMESPACE" 2>/dev/null || echo "⚠ Users API deployment timeout"
kubectl wait --for=condition=available --timeout=300s deployment/ecom-core-api -n "$NAMESPACE" 2>/dev/null || echo "⚠ Core API deployment timeout"
echo ""

# Show deployment status
echo "================================================"
echo "Deployment Status"
echo "================================================"
echo ""
echo "Pods:"
kubectl get pods -n "$NAMESPACE" -o wide

echo ""
echo "Services:"
kubectl get services -n "$NAMESPACE"

echo ""
echo "Ingress:"
kubectl get ingress -n "$NAMESPACE" 2>/dev/null || echo "No ingress found"

echo ""
echo "Deployment logs (last 10 lines):"
echo "Users API:"
kubectl logs -n "$NAMESPACE" deployment/ecom-users-api --tail=10 2>/dev/null || echo "No logs available"

echo ""
echo "Core API:"
kubectl logs -n "$NAMESPACE" deployment/ecom-core-api --tail=10 2>/dev/null || echo "No logs available"

echo ""
echo "================================================"
echo "Deployment completed successfully!"
echo "================================================"
echo ""
echo "Access the APIs:"
echo "Users API: kubectl port-forward -n $NAMESPACE service/ecom-users-api 5001:80"
echo "Core API: kubectl port-forward -n $NAMESPACE service/ecom-core-api 5002:80"
echo ""
echo "To check status: kubectl get all -n $NAMESPACE"
echo "To view logs: kubectl logs -n $NAMESPACE deployment/[deployment-name]"
