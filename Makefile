# Makefile for Ecommerce Microservices

.PHONY: help setup build test deploy clean status logs

# Default target
help:
	@echo "Ecommerce Microservices - Available commands:"
	@echo ""
	@echo "  setup           - Setup Minikube cluster with 2 nodes"
	@echo "  build           - Build Docker images"
	@echo "  test            - Run all tests"
	@echo "  deploy          - Deploy services to Minikube"
	@echo "  status          - Show deployment status"
	@echo "  logs            - Show service logs"
	@echo "  clean           - Clean up resources"
	@echo "  local-dev       - Start local development environment"
	@echo "  local-stop      - Stop local development environment"
	@echo ""

# Setup Minikube cluster
setup:
	@echo "🚀 Setting up Minikube cluster..."
	chmod +x setup-minikube.sh
	./setup-minikube.sh

# Build Docker images
build:
	@echo "🔨 Building Docker images..."
	eval $$(minikube docker-env --profile=ecommerce-cluster) && \
	docker build -t ecom-core-api:latest -f Ecom.Core/src/Ecom.Core.API/Dockerfile . && \
	docker build -t ecom-users-api:latest -f Ecom.Users/src/Ecom.Users.API/Dockerfile .

# Run tests
test:
	@echo "🧪 Running tests..."
	dotnet test Ecom.Core/test/EcomCore.Application.UnitTests/EcomCore.Application.UnitTests.csproj --configuration Release
	dotnet test Ecom.Core/test/EcomCore.Domain.UnitTests/EcomCore.Domain.UnitTests.csproj --configuration Release
	dotnet test Ecom.Core/test/EcomCore.Infrastructure.UnitTests/EcomCore.Infrastructure.UnitTests.csproj --configuration Release
	dotnet test Ecom.Users/test/Ecom.Users.Application.UnitTests/Ecom.Users.Application.UnitTests.csproj --configuration Release
	dotnet test Ecom.Users/test/Ecom.Users.Domain.UnitTests/Ecom.Users.Domain.UnitTests.csproj --configuration Release
	dotnet test Ecom.Users/test/Ecom.Users.Infrastructure.UnitTests/Ecom.Users.Infrastructure.UnitTests.csproj --configuration Release

# Deploy to Minikube
deploy:
	@echo "🚀 Deploying to Minikube..."
	chmod +x deploy.sh
	./deploy.sh

# Show deployment status
status:
	@echo "📊 Deployment Status:"
	@echo "===================="
	kubectl get nodes --show-labels
	@echo ""
	kubectl get deployments -n ecommerce
	@echo ""
	kubectl get pods -n ecommerce -o wide
	@echo ""
	kubectl get services -n ecommerce
	@echo ""
	kubectl get ingress -n ecommerce

# Show service logs
logs:
	@echo "📋 Service Logs:"
	@echo "================"
	@echo "Core API logs:"
	kubectl logs -l app=ecom-core-api -n ecommerce --tail=50
	@echo ""
	@echo "Users API logs:"
	kubectl logs -l app=ecom-users-api -n ecommerce --tail=50

# Clean up resources
clean:
	@echo "🧹 Cleaning up resources..."
	kubectl delete namespace ecommerce --ignore-not-found=true
	minikube delete --profile=ecommerce-cluster || true
	docker image prune -f

# Start local development environment
local-dev:
	@echo "🔧 Starting local development environment..."
	docker-compose -f docker-compose.local.yml up -d --build
	@echo ""
	@echo "Services available at:"
	@echo "  Core API: http://localhost:5001"
	@echo "  Users API: http://localhost:5002"
	@echo "  Gateway: http://localhost:80"

# Stop local development environment
local-stop:
	@echo "🛑 Stopping local development environment..."
	docker-compose -f docker-compose.local.yml down

# Port forward services for testing
port-forward:
	@echo "🔗 Setting up port forwarding..."
	kubectl port-forward -n ecommerce service/ecom-core-service 8080:80 &
	kubectl port-forward -n ecommerce service/ecom-users-service 8081:80 &
	@echo "Services available at:"
	@echo "  Core API: http://localhost:8080"
	@echo "  Users API: http://localhost:8081"
