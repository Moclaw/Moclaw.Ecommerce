# Moclaw E-commerce Platform

A modern, microservices-based e-commerce platform built with .NET 9, featuring comprehensive user management and core e-commerce functionality.

## 🏗️ Architecture Overview

This project follows a **microservices architecture** with **Clean Architecture** principles and **CQRS pattern** implementation:

### Services

- **Ecom.Users** - User management and authentication service
- **EcomCore** - Core e-commerce functionality (products, categories, reviews)
- **AspireHost** - .NET Aspire orchestration host

### Technology Stack

- **.NET 9** - Latest .NET framework
- **PostgreSQL** - Primary database for both services
- **Entity Framework Core** - ORM with Code First approach
- **JWT Authentication** - Secure token-based authentication
- **MediatR** - CQRS and mediator pattern implementation
- **Docker & Docker Compose** - Containerization and orchestration
- **MinimalAPI** - Lightweight API endpoints with attribute routing
- **Autofac** - Dependency injection container
- **Serilog** - Structured logging
- **.NET Aspire** - Cloud-native application development

## 🚀 Features

### User Management (Ecom.Users)
- User registration and authentication
- Role-based access control (RBAC)
- JWT token generation and validation
- User profile management
- Password hashing and security

### E-commerce Core (EcomCore)
- **Product Management**
  - Product CRUD operations
  - Product variants and attributes
  - Product images and galleries
  - SEO-friendly slugs
  - Pricing with sale price support

- **Category Management**
  - Hierarchical category structure
  - Parent-child relationships
  - Category-based product organization

- **Review System**
  - Product reviews and ratings
  - User-generated content
  - Review moderation capabilities

- **Advanced Features**
  - Soft delete implementation
  - Audit trails (CreatedAt, UpdatedAt, DeletedAt)
  - Query filtering and pagination
  - Global query filters for soft-deleted entities

## 📁 Project Structure

```
Moclaw.Ecommerce/
├── AspireHost/                 # .NET Aspire orchestration
│   ├── AspireHost/            # Main orchestration project
│   └── ServiceDefaults1/      # Shared service configurations
├── Ecom.Users/                # User management microservice
│   ├── src/
│   │   ├── Ecom.Users.API/           # API layer
│   │   ├── Ecom.Users.Application/   # Application layer (CQRS)
│   │   ├── Ecom.Users.Domain/        # Domain entities
│   │   ├── Ecom.Users.Infrastructure/ # Data access & external services
│   │   └── Ecom.Users.Shared/        # Shared DTOs and contracts
│   └── test/                         # Unit tests
├── EcomCore/                  # E-commerce core microservice
│   ├── src/
│   │   ├── EcomCore.API/             # API layer
│   │   ├── EcomCore.Application/     # Application layer (CQRS)
│   │   ├── EcomCore.Domain/          # Domain entities
│   │   ├── EcomCore.Infrastructure/  # Data access & external services
│   │   └── EcomCore.Shared/          # Shared DTOs and contracts
│   └── test/                         # Unit tests
├── docker-compose.yml         # Container orchestration
└── Moclaw.Ecommerce.sln      # Solution file
```

## 🔧 Domain Entities

### EcomCore Domain
- **Product** - Core product entity with pricing, SEO, and relationships
- **Category** - Hierarchical category system with parent-child relationships
- **ProductCategory** - Many-to-many relationship between products and categories
- **ProductVariant** - Product variations (size, color, etc.)
- **ProductImage** - Product image management
- **Review** - Customer reviews and ratings
- **Attribute** - Product attributes and specifications

### User Domain
- **User** - User account information
- **Role** - Role-based access control
- **UserRole** - User-role relationships

## 🛠️ Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/) (if running locally)

### Running with Docker Compose

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd Moclaw.Ecommerce
   ```

2. **Start the services**
   ```bash
   docker-compose up -d
   ```

3. **Access the services**
   - User Service API: `http://localhost:5001`
   - EcomCore API: `http://localhost:5002`
   - Swagger Documentation: Available at each service's `/swagger` endpoint

### Running Locally

1. **Set up databases**
   ```bash
   # Create PostgreSQL databases
   createdb Ecom.User
   createdb Ecom.Core
   ```

2. **Update connection strings**
   Update `appsettings.Development.json` in both API projects with your database connection strings.

3. **Run database migrations**
   ```bash
   # Navigate to each API project and run migrations
   cd Ecom.Users/src/Ecom.Users.API
   dotnet ef database update
   
   cd ../../../EcomCore/src/EcomCore.API
   dotnet ef database update
   ```

4. **Run the services**
   ```bash
   # Terminal 1 - User Service
   cd Ecom.Users/src/Ecom.Users.API
   dotnet run
   
   # Terminal 2 - EcomCore Service
   cd EcomCore/src/EcomCore.API
   dotnet run
   ```

### Running with .NET Aspire

1. **Run the AspireHost**
   ```bash
   cd AspireHost/AspireHost
   dotnet run
   ```

2. **Access the Aspire Dashboard**
   Open the URL provided in the console output to access the .NET Aspire dashboard.

## 📚 API Documentation

### Authentication Endpoints (Ecom.Users)
- `POST /auth/register` - User registration
- `POST /auth/login` - User authentication
- `GET /auth/profile` - Get user profile
- `PUT /auth/profile` - Update user profile

### Product Endpoints (EcomCore)
- `GET /products` - Get products with filtering and pagination
- `GET /products/{id}` - Get product by ID
- `POST /products` - Create new product
- `PUT /products/{id}` - Update product
- `DELETE /products/{id}` - Soft delete product
- `GET /products/{id}/images` - Get product images
- `GET /products/{id}/reviews` - Get product reviews

### Category Endpoints (EcomCore)
- `GET /categories` - Get categories with hierarchy
- `GET /categories/{id}` - Get category by ID
- `POST /categories` - Create new category
- `PUT /categories/{id}` - Update category
- `DELETE /categories/{id}` - Soft delete category

### Review Endpoints (EcomCore)
- `GET /reviews` - Get reviews with filtering
- `POST /reviews` - Create new review
- `PUT /reviews/{id}` - Update review
- `DELETE /reviews/{id}` - Soft delete review

## 🔐 Authentication & Authorization

The platform uses **JWT (JSON Web Tokens)** for authentication:

1. **Registration/Login** - Users authenticate through the Ecom.Users service
2. **Token Generation** - JWT tokens are issued upon successful authentication
3. **Token Validation** - All protected endpoints validate JWT tokens
4. **Role-based Access** - Different endpoints require different roles/permissions

### JWT Token Structure
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "role": "customer",
  "exp": 1635724800
}
```

## 🗄️ Database Schema

### PostgreSQL Databases
- **Ecom.User** - User management data
- **Ecom.Core** - E-commerce core data

### Key Features
- **Soft Delete** - Entities are marked as deleted rather than physically removed
- **Audit Fields** - CreatedAt, UpdatedAt, DeletedAt, CreatedBy, UpdatedBy, DeletedBy
- **Global Query Filters** - Automatically filter out soft-deleted entities
- **Indexes** - Optimized for common query patterns

## 🧪 Testing

Run unit tests for each service:

```bash
# User Service Tests
cd Ecom.Users/test
dotnet test

# EcomCore Tests
cd EcomCore/test
dotnet test
```

## 📦 Deployment

### Docker Production Deployment

1. **Build production images**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml build
   ```

2. **Deploy to production**
   ```bash
   docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
   ```

### Environment Variables

Key environment variables to configure:

```env
# Database
CONNECTION_STRING_USER=Host=localhost;Database=Ecom.User;Username=postgres;Password=password
CONNECTION_STRING_CORE=Host=localhost;Database=Ecom.Core;Username=postgres;Password=password

# JWT
JWT_SECRET=your-secret-key
JWT_ISSUER=moclaw-ecommerce
JWT_AUDIENCE=moclaw-api

# Logging
SERILOG_MINIMUMLEVEL=Information
```

## 🔧 Development

### Adding New Features

1. **Domain Layer** - Define entities and domain logic
2. **Application Layer** - Implement CQRS handlers and DTOs
3. **Infrastructure Layer** - Add data access and external service integrations
4. **API Layer** - Create minimal API endpoints
5. **Tests** - Write unit tests for all layers

### Code Standards

- Follow **Clean Architecture** principles
- Use **CQRS pattern** for application logic
- Implement **Repository pattern** for data access
- Apply **SOLID principles**
- Write comprehensive unit tests
- Use consistent naming conventions

## 📄 License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📞 Support

For support and questions, please open an issue in the repository or contact the development team.

---

**Built with ❤️ using .NET 9 and modern software architecture principles**
