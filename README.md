# ProductMesh — Microservices Architecture

A .NET microservices platform built with **Onion Architecture**, **CQRS**, **SOLID principles**, and **12-Factor App** methodology.

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    API Gateway (YARP)                    │
│               :5000 — Rate Limiting + JWT               │
└────────┬──────────────┬──────────────┬──────────────────┘
         │              │              │
    ┌────▼────┐   ┌─────▼─────┐  ┌────▼────┐
    │  Auth   │   │  Product  │  │   Log   │
    │  :5001  │   │   :5002   │  │  :5003  │
    │         │   │  (CQRS)   │  │         │
    └────┬────┘   └──┬────┬───┘  └────┬────┘
         │          │    │           │
    ┌────▼────┐ ┌───▼──┐ │     ┌────▼────┐
    │SQL Server│ │Redis │ │     │RabbitMQ │◄──── Event Bus
    │ (Auth)  │ │Cache │ │     │         │
    └─────────┘ └──────┘ │     └─────────┘
                    ┌─────▼─────┐
                    │ SQL Server│
                    │(Product/Log)│
                    └───────────┘
```

### Microservices

| Service | Port | Responsibilities |
|---------|------|-----------------|
| **API Gateway** | 5000 | YARP reverse proxy, rate limiting (sliding window), centralized JWT auth, CORS |
| **Auth Service** | 5001 | JWT token generation, refresh token rotation, Microsoft Identity, role/policy-based authorization |
| **Product Service** | 5002 | CQRS (MediatR), Redis cache-aside, event publishing, product CRUD |
| **Log Service** | 5003 | Centralized structured logging, RabbitMQ consumers, log querying |

### Design Patterns

- **Onion Architecture** — Domain → Application → Infrastructure → API (dependencies point inward)
- **CQRS** — Commands (write) and Queries (read) separated via MediatR
- **SAGA Pattern** — Choreography-based distributed transactions for product creation flow
- **Event-Driven** — RabbitMQ + MassTransit for async cross-service communication
- **Cache-Aside** — Redis for product query optimization with cache invalidation
- **Repository + Unit of Work** — Data access abstraction

## Tech Stack

| Technology | Usage |
|-----------|-------|
| .NET 10 | Framework |
| Entity Framework Core | ORM |
| SQL Server | Database |
| Redis | Cache |
| RabbitMQ | Message broker |
| MassTransit | Event bus abstraction |
| MediatR | CQRS dispatcher |
| YARP | API Gateway / reverse proxy |
| Serilog + Seq | Structured logging |
| FluentValidation | Request validation |
| Docker | Containerization |
| GitHub Actions | CI/CD |

## Getting Started

### Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/)
- [Docker & Docker Compose](https://docs.docker.com/get-docker/)

### Quick Start (Docker)

```bash
# Start all infrastructure and services
cd docker
docker compose up -d

# Services will be available at:
# Gateway:  http://localhost:5000
# Auth:     http://localhost:5001
# Product:  http://localhost:5002
# Log:      http://localhost:5003
# Seq UI:   http://localhost:8081
# RabbitMQ: http://localhost:15672 (guest/guest)
```

### Local Development

```bash
# 1. Start infrastructure only
cd docker
docker compose up -d sqlserver redis rabbitmq seq

# 2. Build solution
cd ..
dotnet build

# 3. Run services (each in a separate terminal)
dotnet run --project src/Services/Auth/ProductMesh.Auth.API
dotnet run --project src/Services/Product/ProductMesh.Product.API
dotnet run --project src/Services/Log/ProductMesh.Log.API
dotnet run --project src/ApiGateway/ProductMesh.Gateway
```

## API Endpoints

### Auth (`/auth`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/auth/register` | No | Register a new user |
| POST | `/auth/login` | No | Login and receive JWT + refresh token |
| POST | `/auth/refresh` | No | Refresh expired access token |
| POST | `/auth/revoke` | Yes | Revoke refresh token |

### Products (`/api/products`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/products` | No | List products (Redis cached) |
| GET | `/api/products/{id}` | No | Get product by ID (Redis cached) |
| POST | `/api/products` | Yes (JWT) | Create product |
| PUT | `/api/products/{id}` | Yes (JWT) | Update product |

### Logs (`/api/logs`)
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/logs?serviceName=&level=&from=&to=` | Admin | Query logs with filters |
| GET | `/api/logs/{id}` | Admin | Get log entry detail |

### Default Admin Credentials
- **Email:** admin@productmesh.com
- **Password:** Admin123!

## Project Structure

```
ProductMesh/
├── src/
│   ├── ApiGateway/ProductMesh.Gateway/          # YARP + Rate Limiting
│   ├── Services/
│   │   ├── Auth/                                # Onion: Domain/Application/Infrastructure/API
│   │   ├── Product/                             # Onion + CQRS: Commands/Queries via MediatR
│   │   └── Log/                                 # Centralized structured logging
│   └── Shared/ProductMesh.Shared/               # Event contracts, base entities, interfaces
├── tests/
├── docker/
│   ├── docker-compose.yml
│   └── Dockerfile.{auth,product,log,gateway}
├── .github/workflows/ci-cd.yml
└── ProductMesh.sln
```

## 12-Factor Compliance

| Factor | Implementation |
|--------|---------------|
| Codebase | Single repo, version controlled |
| Dependencies | NuGet packages, explicitly declared |
| Config | Environment variables (`appsettings.json` + env overrides) |
| Backing Services | SQL Server, Redis, RabbitMQ as attached resources |
| Build/Release/Run | Docker multi-stage builds, separate stages |
| Stateless Processes | No server-side session state, JWT for auth |
| Port Binding | Each service self-contained with own port |
| Concurrency | Async/await, horizontal scaling via containers |
| Disposability | Graceful shutdown via `IHostedService` lifecycle |
| Dev/Prod Parity | Docker Compose for consistent environments |
| Logs | Serilog structured JSON → stdout + Seq |
| Admin Processes | EF Core migrations, seed data on startup |

## Branching Strategy

- `test/v1.0.0` — Development and testing
- `prod/v1.0.0` — Production release (merged from test branch)

## CI/CD

GitHub Actions pipeline triggers on `test/**` and `prod/**` branches:
1. **Restore** → **Build** → **Test** (matrix build per service)
2. **Docker Build** (on prod branches)

## License

Private — Case Study Project
