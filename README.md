# Membera

**Membera** is a subscription and QR-redemption platform for local businesses. Restaurants, salons, and other service providers create subscription plans; customers buy a plan once and redeem it by scanning a QR code — no app, no punch cards, just a fast scan.

This repository is a portfolio project built with a real microservices architecture — Auth and Merchant are independent services that communicate asynchronously through RabbitMQ, each with its own database, following Clean Architecture principles throughout.

---

## Tech Stack

**Backend**
- .NET 8, C#
- Clean Architecture (Domain / Application / Infrastructure / Api layers per service)
- PostgreSQL (one database per microservice)
- RabbitMQ (event-driven communication between services)
- Redis (distributed caching)
- Entity Framework Core
- JWT authentication + Google OAuth
- FluentValidation
- Serilog (structured logging)
- xUnit + Moq (65 unit tests)

**Frontend**
- React, TypeScript, Tailwind CSS

**Infrastructure**
- Docker & Docker Compose (PostgreSQL, RabbitMQ, Redis)

---

## Key Features

- **Two independent microservices** — Auth and Merchant, each with its own database, deployable and scalable separately
- **Event-driven architecture** — when a user registers as a merchant owner, Auth publishes a `UserRegisteredEvent` to RabbitMQ; the Merchant service consumes it and automatically creates a merchant profile, with no direct coupling between the services
- **Authentication** — email/password with BCrypt hashing, Google OAuth, JWT access + refresh tokens, role-based access control (User, MerchantOwner, Admin, SuperAdmin)
- **Distributed caching** — Redis cache-aside pattern on read-heavy endpoints, with automatic invalidation on writes
- **Structured logging** — Serilog across both services, writing to console and rolling daily log files
- **65 automated tests** covering every handler's success paths and failure branches
- **Global exception handling** and a consistent API response format across both services

---

## Architecture

```
┌─────────────────┐         ┌──────────────────┐
│   Auth Service   │         │ Merchant Service  │
│  (Membera.Auth)  │         │(Membera.Merchant) │
│                  │         │                   │
│  Own PostgreSQL  │         │  Own PostgreSQL   │
│   database       │         │   database        │
└────────┬─────────┘         └─────────▲─────────┘
         │                             │
         │      publishes event        │ consumes event
         └──────────► RabbitMQ ────────┘
                    (membera.events)

         Both services also use:
         → Redis (caching)
         → Shared building blocks (BaseEntity, BaseResponse, messaging contracts)
```

Each service follows the same internal structure:

```
Membera.[Service].Domain          → Entities, business rules, no dependencies
Membera.[Service].Application     → Use cases (Command/Handler pattern), interfaces
Membera.[Service].Infrastructure  → EF Core, repositories, external services
Membera.[Service].Api             → Controllers, DI configuration
```

Services never share a database. They only communicate through RabbitMQ events, which is how a real microservices system stays loosely coupled.

---

## Getting Started

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- Visual Studio 2022 (or any IDE that supports .NET)

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/membera.git
cd membera/membera-backend
```

### 2. Set up environment variables

Copy the example file and fill in your own values:

```bash
cp .env.example .env
```

Edit `.env` and set:
- `POSTGRES_PASSWORD`
- `JWT_SECRET_KEY` (at least 32 characters)
- `JWT_ISSUER` / `JWT_AUDIENCE`

### 3. Start the infrastructure (PostgreSQL, RabbitMQ, Redis)

```bash
docker-compose up -d
```

### 4. Apply database migrations

From Visual Studio's Package Manager Console, for each service:

```powershell
Update-Database -Project Membera.Auth.Infrastructure -StartupProject Membera.Auth.Api
Update-Database -Project Membera.Merchant.Infrastructure -StartupProject Membera.Merchant.Api
```

### 5. Run both services

Set both `Membera.Auth.Api` and `Membera.Merchant.Api` as startup projects (Solution Properties → Multiple startup projects) and run. Each opens its own Swagger UI.

---

## Testing

```bash
cd membera-backend
dotnet test
```

This runs all 65 unit tests across both services (Auth: 57, Merchant: 8), covering every handler's success and failure paths using xUnit and Moq.

---

## Project Status

This is an active, in-progress portfolio project. Auth and Merchant services are functional; subscription plans, payment integration (Stripe), QR redemption, an API Gateway, and full containerization are in progress.
