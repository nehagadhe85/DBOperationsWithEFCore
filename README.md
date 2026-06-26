# Library Management System

A full-stack Library Management System built with **.NET 8 Web API + Entity Framework Core** (backend) and **React + Vite** (frontend). Designed as a learning project to understand EF Core and .NET API concepts.

## Architecture

```
DBOperationsWithEFCore/           ← .NET 8 Web API Backend
├── Models/                       ← EF Core Entity models
├── Data/                         ← DbContext + Fluent API configurations + Seed Data
├── DTOs/                         ← Request/Response DTOs
├── Services/                     ← Business logic with EF Core queries
├── Controllers/                  ← REST API endpoints
├── Middleware/                   ← Global error handling
└── Migrations/                   ← EF Core auto-generated migrations

library-client/                   ← React Frontend (Vite)
├── src/components/               ← Reusable UI components
├── src/pages/                    ← Page-level components
├── src/services/                 ← API service layer (axios)
└── src/hooks/                    ← Custom React hooks
```

## EF Core Concepts Covered

| Concept | Location |
|---------|----------|
| DbContext & DbSet | `Data/LibraryDbContext.cs` |
| Data Annotations | `Models/Book.cs`, `Models/Member.cs` |
| Fluent API | `Data/EntityConfigurations/BookConfiguration.cs` |
| Migrations | `Migrations/` folder |
| One-to-Many Relationships | Author → Books, Category → Books |
| Eager Loading (`.Include()`) | `Services/BookService.cs` |
| `AsNoTracking()` | All read-only queries |
| Global Query Filters (Soft Delete) | `BookConfiguration.cs` |
| Value Conversions (Enum → String) | `LoanStatus` in `LibraryDbContext.cs` |
| Seed Data | `LibraryDbContext.cs` |
| Transactions | `Services/LoanService.cs` |
| LINQ Pagination (Skip/Take) | `Services/BookService.cs` |

## Prerequisites

- .NET 8 SDK
- Node.js 16+
- SQL Server LocalDB (comes with Visual Studio)

## Getting Started

### Backend

```bash
# Restore packages
dotnet restore

# Create & apply database migration
dotnet ef migrations add InitialCreate
dotnet ef database update

# Run the API (opens on https://localhost:5001)
dotnet run
```

### Frontend

```bash
cd library-client

# Install dependencies
npm install

# Run dev server (opens on http://localhost:5173)
npm run dev
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/books?page=1&search=...` | List books (paginated, searchable) |
| POST | `/api/books` | Create a book |
| PUT | `/api/books/{id}` | Update a book |
| DELETE | `/api/books/{id}` | Soft-delete a book |
| GET | `/api/authors` | List all authors |
| GET | `/api/categories` | List all categories |
| GET | `/api/members?page=1` | List members (paginated) |
| POST | `/api/loans/checkout` | Checkout a book |
| PUT | `/api/loans/{id}/return` | Return a book |
| GET | `/api/loans/dashboard` | Dashboard statistics |

Swagger UI available at: `https://localhost:5001/swagger`