#!/bin/bash

# Cleanup Kubernetes resources
# Usage: ./cleanup-k8s.sh [namespace]

NAMESPACE=${1:-"ecommerce"}

echo "Cleaning up Kubernetes resources in namespace: $NAMESPACE"

# Delete deployments
kubectl delete deployment ecom-users-api -n "$NAMESPACE" 2>/dev/null || true
kubectl delete deployment ecom-core-api -n "$NAMESPACE" 2>/dev/null || true

# Delete services
kubectl delete service ecom-users-api -n "$NAMESPACE" 2>/dev/null || true
kubectl delete service ecom-core-api -n "$NAMESPACE" 2>/dev/null || true

# Delete ingress
kubectl delete ingress ecom-ingress -n "$NAMESPACE" 2>/dev/null || true

# Delete configmaps and secrets
kubectl delete configmap ecom-config -n "$NAMESPACE" 2>/dev/null || true
kubectl delete secret ecom-secrets -n "$NAMESPACE" 2>/dev/null || true

echo "Cleanup completed!"
