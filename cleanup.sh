#!/bin/bash

# Cleanup script for Ecommerce Minikube environment
set -e

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

echo "🧹 Cleaning up Ecommerce Minikube environment..."

# Function to clean namespace resources
cleanup_namespace() {
    if kubectl get namespace $NAMESPACE &> /dev/null; then
        print_status "Cleaning up namespace $NAMESPACE..."
        
        # Delete deployments
        kubectl delete deployments --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete services
        kubectl delete services --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete ingress
        kubectl delete ingress --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete HPA
        kubectl delete hpa --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete network policies
        kubectl delete networkpolicies --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete PVCs
        kubectl delete pvc --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete secrets
        kubectl delete secrets --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Delete configmaps
        kubectl delete configmaps --all -n $NAMESPACE --timeout=60s 2>/dev/null || true
        
        # Finally delete namespace
        kubectl delete namespace $NAMESPACE --timeout=60s 2>/dev/null || true
        
        print_status "Namespace $NAMESPACE cleaned up"
    else
        print_warning "Namespace $NAMESPACE not found"
    fi
}

# Check cleanup type
case "${1:-partial}" in
    "full")
        print_warning "Performing full cleanup - deleting entire Minikube cluster"
        read -p "Are you sure you want to delete the entire cluster '$PROFILE'? (y/N): " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            minikube delete --profile=$PROFILE
            print_status "Cluster '$PROFILE' deleted completely"
        else
            print_status "Full cleanup cancelled"
        fi
        ;;
    "partial"|"")
        print_status "Performing partial cleanup - removing deployed resources only"
        cleanup_namespace
        
        # Clean Docker images if Minikube is running
        if minikube status --profile=$PROFILE &> /dev/null; then
            print_status "Cleaning up Docker images..."
            eval $(minikube docker-env --profile=$PROFILE)
            
            # Remove ecommerce images
            docker images | grep ecom | awk '{print $3}' | xargs -r docker rmi -f 2>/dev/null || true
            
            # Clean unused images
            docker system prune -f 2>/dev/null || true
            
            print_status "Docker cleanup completed"
        fi
        ;;
    "images")
        print_status "Cleaning up Docker images only..."
        if minikube status --profile=$PROFILE &> /dev/null; then
            eval $(minikube docker-env --profile=$PROFILE)
            docker images | grep ecom | awk '{print $3}' | xargs -r docker rmi -f 2>/dev/null || true
            docker system prune -f 2>/dev/null || true
            print_status "Docker images cleaned up"
        else
            print_error "Minikube cluster is not running"
        fi
        ;;
    *)
        echo "Usage: $0 [partial|full|images]"
        echo "  partial: Remove deployed resources only (default)"
        echo "  full:    Delete entire Minikube cluster"
        echo "  images:  Clean up Docker images only"
        exit 1
        ;;
esac

print_status "Cleanup completed!"

# Show remaining resources if cluster still exists
if minikube status --profile=$PROFILE &> /dev/null; then
    echo ""
    echo "📊 Remaining cluster resources:"
    kubectl get all --all-namespaces 2>/dev/null || true
fi
