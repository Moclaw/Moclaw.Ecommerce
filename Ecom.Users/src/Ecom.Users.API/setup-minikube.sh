#!/bin/bash

echo "Setting up Minikube cluster for ecommerce services..."

# Start Minikube with multiple nodes
minikube start --nodes 2 --driver=docker --kubernetes-version=v1.28.0

# Enable required addons
minikube addons enable registry
minikube addons enable ingress

# Label nodes for service affinity
kubectl label nodes minikube service=users
kubectl label nodes minikube-m02 service=core

echo "Minikube cluster setup complete!"
echo "Nodes:"
kubectl get nodes --show-labels

echo "To build and deploy:"
echo "1. Run: eval \$(minikube docker-env)"
echo "2. Run: docker-compose build"
echo "3. Run: kubectl apply -f k8s/"
