# Makefile for Ecommerce Microservices

.PHONY: help build test deploy clean status logs

# Default target
help:
	@echo "Ecommerce Microservices - Available commands:"
	@echo ""
	@echo "  build           - Build Docker images"
	@echo "  test            - Run all tests"
	@echo "  deploy          - Deploy services with Docker Compose"
	@echo "  status          - Show deployment status"
	@echo "  logs            - Show service logs"
	@echo "  clean           - Clean up resources"
	@echo "  local-dev       - Start local development environment"
	@echo "  local-stop      - Stop local development environment"
	@echo ""

# Build Docker images
build:
	@echo "🔨 Building Docker images..."
	docker-compose build

# Run tests
test:
	@echo "🧪 Running tests..."
	dotnet test --configuration Release --verbosity minimal

# Deploy services with Docker Compose
deploy:
	@echo "🚀 Deploying services with Docker Compose..."
	docker-compose up -d --build
	@echo ""
	@echo "Services available at:"
	@echo "  Gateway API: http://localhost:5300"
	@echo "  Core API (via Gateway): http://localhost:5300/api/core"
	@echo "  Users API (via Gateway): http://localhost:5300/api/users"
	@echo "  Core API (Direct): http://localhost:5301"
	@echo "  Users API (Direct): http://localhost:5302"
	@echo "  Prometheus: http://localhost:9090"
	@echo "  Grafana: http://localhost:3000"

# Show deployment status
status:
	@echo "📊 Checking service status..."
	docker-compose ps

# Show service logs
logs:
	@echo "📋 Showing service logs..."
	docker-compose logs -f

# Clean up resources
clean:
	@echo "🧹 Cleaning up resources..."
	docker-compose down --volumes --remove-orphans
	docker image prune -f

# Start local development environment
local-dev:
	@echo "🔧 Starting local development environment..."
	docker-compose -f docker-compose.yml up -d --build
	@echo ""
	@echo "Services available at:"
	@echo "  Gateway API: http://localhost:5300"
	@echo "  Core API (via Gateway): http://localhost:5300/api/core"
	@echo "  Users API (via Gateway): http://localhost:5300/api/users"
	@echo "  Core API (Direct): http://localhost:5301"
	@echo "  Users API (Direct): http://localhost:5302"
	@echo "  Prometheus: http://localhost:9090"
	@echo "  Grafana: http://localhost:3000"

# Stop local development environment
local-stop:
	@echo "🛑 Stopping local development environment..."
	docker-compose down

# Health checks
health-check:
	@echo "� Performing health checks..."
	@curl -s http://localhost:5300/health && echo "✅ Gateway API: Healthy" || echo "❌ Gateway API: Failed"
	@curl -s http://localhost:5301/health && echo "✅ Core API: Healthy" || echo "❌ Core API: Failed"
	@curl -s http://localhost:5302/health && echo "✅ Users API: Healthy" || echo "❌ Users API: Failed"

# Restart specific service
restart-gateway:
	@echo "🔄 Restarting Gateway service..."
	docker-compose restart ecom.gateway.api

restart-core:
	@echo "🔄 Restarting Core API service..."
	docker-compose restart ecom.core.api

restart-users:
	@echo "🔄 Restarting Users API service..."
	docker-compose restart ecom.users.api
