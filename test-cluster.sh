#!/bin/bash

# Comprehensive testing script for Ecommerce microservices on Minikube
set -e

# Configuration
PROFILE="ecommerce-cluster"
NAMESPACE="ecommerce"
TIMEOUT=300

# Colors
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

print_header() {
    echo -e "${BLUE}=== $1 ===${NC}"
}

print_status() {
    echo -e "${GREEN}[✓]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[⚠]${NC} $1"
}

print_error() {
    echo -e "${RED}[✗]${NC} $1"
}

# Test function
test_endpoint() {
    local SERVICE_NAME=$1
    local PORT=$2
    local ENDPOINT=$3
    local EXPECTED_STATUS=$4
    
    print_header "Testing $SERVICE_NAME"
    
    # Port forward in background
    kubectl port-forward -n $NAMESPACE service/$SERVICE_NAME $PORT:80 &
    PF_PID=$!
    
    # Wait for port forward to be ready
    sleep 5
    
    # Test the endpoint
    RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:$PORT$ENDPOINT || echo "000")
    
    if [ "$RESPONSE" = "$EXPECTED_STATUS" ]; then
        print_status "$SERVICE_NAME responding correctly (HTTP $RESPONSE)"
    else
        print_error "$SERVICE_NAME test failed (HTTP $RESPONSE, expected $EXPECTED_STATUS)"
    fi
    
    # Clean up port forward
    kill $PF_PID 2>/dev/null || true
    sleep 2
}

# Check if cluster is running
if ! minikube status --profile=$PROFILE &> /dev/null; then
    print_error "Minikube cluster '$PROFILE' is not running"
    exit 1
fi

print_header "Cluster Health Check"

# Check node status
print_status "Checking node status..."
kubectl get nodes --show-labels

# Check namespace
print_status "Checking namespace..."
kubectl get namespace $NAMESPACE

# Check deployments
print_header "Deployment Status"
kubectl get deployments -n $NAMESPACE
kubectl get pods -n $NAMESPACE -o wide

# Check services
print_header "Service Status"
kubectl get services -n $NAMESPACE

# Check node distribution
print_header "Node Distribution Analysis"
echo "Core service pods:"
kubectl get pods -n $NAMESPACE -l app=ecom-core-api -o wide
echo ""
echo "Users service pods:"
kubectl get pods -n $NAMESPACE -l app=ecom-users-api -o wide

# Verify pods are on different nodes if multi-node setup
CORE_NODES=$(kubectl get pods -n $NAMESPACE -l app=ecom-core-api -o jsonpath='{.items[*].spec.nodeName}' | tr ' ' '\n' | sort -u)
USERS_NODES=$(kubectl get pods -n $NAMESPACE -l app=ecom-users-api -o jsonpath='{.items[*].spec.nodeName}' | tr ' ' '\n' | sort -u)

echo ""
echo "Core service running on nodes: $CORE_NODES"
echo "Users service running on nodes: $USERS_NODES"

# Check if services are distributed across nodes
TOTAL_NODES=$(kubectl get nodes --no-headers | wc -l)
if [ $TOTAL_NODES -gt 1 ]; then
    if [ "$CORE_NODES" != "$USERS_NODES" ]; then
        print_status "Services are distributed across different nodes"
    else
        print_warning "Services are running on the same node(s)"
    fi
else
    print_warning "Single node cluster - services will share the same node"
fi

# Test endpoints
print_header "Endpoint Testing"

# Wait for pods to be ready
kubectl wait --for=condition=ready pod -l app=ecom-core-api -n $NAMESPACE --timeout=${TIMEOUT}s
kubectl wait --for=condition=ready pod -l app=ecom-users-api -n $NAMESPACE --timeout=${TIMEOUT}s

# Test Core service
test_endpoint "ecom-core-service" "8080" "/health" "200"

# Test Users service  
test_endpoint "ecom-users-service" "8081" "/health" "200"

# Test ingress (if available)
print_header "Ingress Testing"
MINIKUBE_IP=$(minikube ip --profile=$PROFILE)
echo "Minikube IP: $MINIKUBE_IP"

if kubectl get ingress -n $NAMESPACE &> /dev/null; then
    kubectl get ingress -n $NAMESPACE
    
    # Add to /etc/hosts instructions
    echo ""
    print_status "To test ingress, add these entries to your /etc/hosts:"
    echo "$MINIKUBE_IP core.ecommerce.local"
    echo "$MINIKUBE_IP users.ecommerce.local"
    echo ""
    echo "Then test with:"
    echo "curl http://core.ecommerce.local/health"
    echo "curl http://users.ecommerce.local/health"
fi

# Resource usage
print_header "Resource Usage"
kubectl top nodes 2>/dev/null || print_warning "Metrics server not available"
kubectl top pods -n $NAMESPACE 2>/dev/null || print_warning "Pod metrics not available"

# HPA status
print_header "Horizontal Pod Autoscaler"
kubectl get hpa -n $NAMESPACE 2>/dev/null || print_warning "HPA not configured"

# Network policies
print_header "Network Policies"
kubectl get networkpolicies -n $NAMESPACE 2>/dev/null || print_warning "Network policies not found"

print_header "Test Summary"
print_status "Cluster testing completed"
echo ""
echo "📊 Quick access commands:"
echo "  minikube dashboard --profile=$PROFILE"
echo "  kubectl logs -f deployment/ecom-core-api -n $NAMESPACE"
echo "  kubectl logs -f deployment/ecom-users-api -n $NAMESPACE"
echo "  kubectl get events -n $NAMESPACE --sort-by='.lastTimestamp'"
