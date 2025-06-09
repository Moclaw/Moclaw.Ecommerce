#!/bin/bash

echo "Deploying ecommerce services to Minikube..."

# Use Minikube's Docker daemon
eval $(minikube docker-env)

# Build Docker images
echo "Building Docker images..."
docker build -t ecom-users-api:latest -f Ecom.Users/src/Ecom.Users.API/Dockerfile .
docker build -t ecom-core-api:latest -f Ecom.Core/src/Ecom.Core.API/Dockerfile .

# Apply Kubernetes manifests
echo "Applying Kubernetes manifests..."
kubectl apply -f k8s/namespace.yml
kubectl apply -f k8s/users-service/
kubectl apply -f k8s/core-service/

# Wait for deployments to be ready
echo "Waiting for deployments..."
kubectl wait --for=condition=available --timeout=300s deployment/ecom-users-api -n ecommerce
kubectl wait --for=condition=available --timeout=300s deployment/ecom-core-api -n ecommerce

echo "Deployment complete!"
echo "Service URLs:"
minikube service ecom-users-service --url -n ecommerce
minikube service ecom-core-service --url -n ecommerce
