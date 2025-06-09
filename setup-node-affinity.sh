#!/bin/bash

# Node Affinity Setup Script for Ecommerce Services
# This script ensures each service runs on dedicated nodes

set -e

PROFILE="ecommerce-cluster"
NAMESPACE="ecommerce"

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

# Check if cluster is running
if ! minikube status --profile=$PROFILE &> /dev/null; then
    print_error "Minikube cluster '$PROFILE' is not running"
    print_status "Run './setup-minikube.sh' first"
    exit 1
fi

print_header "Node Affinity Configuration"

# Get all nodes
NODES=($(kubectl get nodes -o jsonpath='{.items[*].metadata.name}'))
NODE_COUNT=${#NODES[@]}

print_status "Found $NODE_COUNT node(s): ${NODES[*]}"

if [ $NODE_COUNT -eq 1 ]; then
    print_warning "Single node cluster detected"
    print_status "Configuring node for both services with priority scheduling"
    
    # Label single node for both services but with different priorities
    kubectl label nodes ${NODES[0]} service=core --overwrite
    kubectl label nodes ${NODES[0]} service-priority=core --overwrite
    kubectl label nodes ${NODES[0]} users-node=secondary --overwrite
    
    print_status "Node ${NODES[0]} configured for core service (primary) and users service (secondary)"
    
elif [ $NODE_COUNT -eq 2 ]; then
    print_status "Two-node cluster detected - ideal for service separation"
    
    # Assign services to specific nodes
    kubectl label nodes ${NODES[0]} service=core --overwrite
    kubectl label nodes ${NODES[0]} node-role=core-primary --overwrite
    kubectl label nodes ${NODES[1]} service=users --overwrite
    kubectl label nodes ${NODES[1]} node-role=users-primary --overwrite
    
    print_status "Node ${NODES[0]} dedicated to Core service"
    print_status "Node ${NODES[1]} dedicated to Users service"
    
else
    print_status "Multi-node cluster detected ($NODE_COUNT nodes)"
    
    # For clusters with 3+ nodes, use first two for services, others for system workloads
    kubectl label nodes ${NODES[0]} service=core --overwrite
    kubectl label nodes ${NODES[0]} node-role=core-primary --overwrite
    kubectl label nodes ${NODES[1]} service=users --overwrite
    kubectl label nodes ${NODES[1]} node-role=users-primary --overwrite
    
    # Label remaining nodes for system workloads
    for ((i=2; i<$NODE_COUNT; i++)); do
        kubectl label nodes ${NODES[$i]} node-role=system --overwrite
        kubectl label nodes ${NODES[$i]} service- --overwrite 2>/dev/null || true
    done
    
    print_status "Nodes ${NODES[0]} and ${NODES[1]} dedicated to services"
    print_status "Remaining nodes reserved for system workloads"
fi

# Add node capacity labels for monitoring
for node in "${NODES[@]}"; do
    # Get node capacity
    CPU_CAPACITY=$(kubectl get node $node -o jsonpath='{.status.capacity.cpu}')
    MEMORY_CAPACITY=$(kubectl get node $node -o jsonpath='{.status.capacity.memory}')
    
    kubectl label nodes $node cpu-capacity=$CPU_CAPACITY --overwrite
    kubectl label nodes $node memory-capacity=$MEMORY_CAPACITY --overwrite
done

print_header "Node Configuration Summary"
kubectl get nodes --show-labels

print_header "Verifying Node Assignments"

# Wait a moment for labels to propagate
sleep 2

# Check if pods are scheduled correctly
if kubectl get pods -n $NAMESPACE &> /dev/null; then
    echo "Current pod distribution:"
    kubectl get pods -n $NAMESPACE -o wide
    
    # Check core service pods
    CORE_PODS=$(kubectl get pods -n $NAMESPACE -l app=ecom-core-api -o jsonpath='{.items[*].spec.nodeName}' 2>/dev/null || echo "")
    if [ ! -z "$CORE_PODS" ]; then
        echo "Core service pods on nodes: $CORE_PODS"
    fi
    
    # Check users service pods
    USERS_PODS=$(kubectl get pods -n $NAMESPACE -l app=ecom-users-api -o jsonpath='{.items[*].spec.nodeName}' 2>/dev/null || echo "")
    if [ ! -z "$USERS_PODS" ]; then
        echo "Users service pods on nodes: $USERS_PODS"
    fi
    
    # Restart deployments to apply new node affinity
    print_status "Restarting deployments to apply node affinity..."
    kubectl rollout restart deployment/ecom-core-api -n $NAMESPACE 2>/dev/null || print_warning "Core deployment not found"
    kubectl rollout restart deployment/ecom-users-api -n $NAMESPACE 2>/dev/null || print_warning "Users deployment not found"
    
    # Wait for rollout to complete
    kubectl rollout status deployment/ecom-core-api -n $NAMESPACE --timeout=300s 2>/dev/null || true
    kubectl rollout status deployment/ecom-users-api -n $NAMESPACE --timeout=300s 2>/dev/null || true
else
    print_warning "No pods found in namespace $NAMESPACE"
    print_status "Deploy services using './deploy.sh' to see node affinity in action"
fi

print_header "Node Affinity Rules Applied"
print_status "Services will be scheduled according to node labels"
print_status "Use './test-cluster.sh' to verify the distribution"

echo ""
echo "📊 Node Assignment Strategy:"
if [ $NODE_COUNT -eq 1 ]; then
    echo "  • Single node: Both services with priority scheduling"
elif [ $NODE_COUNT -eq 2 ]; then
    echo "  • Node 1 (${NODES[0]}): Core Service"
    echo "  • Node 2 (${NODES[1]}): Users Service"
else
    echo "  • Node 1 (${NODES[0]}): Core Service"
    echo "  • Node 2 (${NODES[1]}): Users Service"
    echo "  • Other nodes: System workloads"
fi
