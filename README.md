# CRUD App (.NET 10 + Vue 3 + MySQL)

Full-stack CRUD application with Docker Compose and Kubernetes support.

**Stack:**

- API: .NET 10 Web API, EF Core, LINQ Query Syntax, Pomelo MySQL provider
- Client: Vue 3 (Vite)
- Database: MySQL 8
- DB Admin: Adminer

## Why Docker and Kubernetes?

### Docker Compose (Recommended for Development)

- **Simple**: One command to start everything
- **Fast**: Perfect for local development
- **Easy setup**: No cluster needed

### Kubernetes (For Production/Testing)

- **Scalable**: Run multiple replicas, handle traffic spikes
- **Resilient**: Auto-restarts failed pods, health checks
- **Production-ready**: Used by most cloud providers
- **Learning**: Practice real-world deployment

**Quick Decision:**

- **Learning/Development?** → Use Docker Compose
- **Production/Scaling?** → Use Kubernetes
- **Both work!** → This project supports both

## Prerequisites

**For Docker Compose:**

- Docker Desktop installed and running

**For Local Development (Without Docker):**

- .NET SDK 10.0
- Node.js 18+ and npm
- MySQL 8.0 (optional - can use Docker MySQL on port 3308)

**For Kubernetes:**

- Docker Desktop with Kubernetes enabled, OR
- Minikube/kind cluster, OR
- Any Kubernetes cluster (EKS, GKE, AKS, etc.)

## Quick Start: Docker Compose (Easiest)

### 🚀 First Time Setup (One-time only)

**1. Generate SSL certificates (required for HTTPS)**

On Windows (PowerShell):

```powershell
.\nginx\generate-ssl.ps1
```

On Linux/Mac (Bash):

```bash
chmod +x nginx/generate-ssl.sh
./nginx/generate-ssl.sh
```

**2. Build and start all services (first time)**

```bash
docker compose -p crudapp up -d --build
```

⏳ **Wait 10-30 seconds** for all services to start (database initialization takes time)

---

### ⚡ Running Again (After first setup)

**Just start the services** (no SSL generation or build needed):

```bash
docker compose -p crudapp up -d
```

That's it! Everything will start using existing images and certificates.

---

### 🌐 Access Your Apps

Once services are running:

- **Client (Vue)**: **https://localhost:5443** (or http://localhost:5173 redirects to HTTPS)
- **API (Swagger)**: **https://localhost:8443/swagger** (or http://localhost:8080 redirects to HTTPS)
- **Adminer (DB UI)**: http://localhost:8081
  - System: MySQL
  - Server: `db` (when using Docker Compose)
  - Username: `root`
  - Password: `secret`
  - Database: `crudapp`

**For Local Development:**

- If running API with `dotnet run`, use Adminer with:
  - Server: `host.docker.internal:3308` (when accessing Docker MySQL from Adminer)
  - Or connect directly to local MySQL on `localhost:3306`

**Note:** You may see a browser warning about self-signed certificates. Click "Advanced" → "Proceed to localhost" to continue. This is normal for development certificates.

---

### 🛠️ Useful Commands

```bash
# Check if services are running
docker compose -p crudapp ps

# View logs (all services)
docker compose -p crudapp logs -f

# View logs for specific service
docker compose -p crudapp logs -f api
docker compose -p crudapp logs -f client

# View logs by container name
docker logs vuejs-csharp-api
docker logs vuejs-csharp-db
docker logs vuejs-csharp-client

# Stop everything (keeps data)
docker compose -p crudapp down

# Stop and delete everything including data (⚠️ WARNING: Deletes database)
docker compose -p crudapp down -v

# Rebuild if you changed code
docker compose -p crudapp up -d --build

# Restart a specific service
docker compose -p crudapp restart api

# Execute commands inside containers
docker exec -it vuejs-csharp-api bash
docker exec -it vuejs-csharp-db mysql -uroot -psecret
```

**Container Names:**

- Database: `vuejs-csharp-db`
- Adminer: `vuejs-csharp-adminer`
- API: `vuejs-csharp-api`
- API Nginx: `vuejs-csharp-api-nginx`
- Client: `vuejs-csharp-client`

## Database Migrations & Seed

### Automatic Setup (Default)

**On startup:** The API automatically:

1. Applies EF Core migrations using `Database.MigrateAsync()` (creates/updates database schema)
2. Seeds 3 sample products (Laptop, Mouse, Keyboard) if the table is empty

**How it works:**

- `Database.MigrateAsync()` runs on API startup to apply pending migrations (see `backend/Program.cs`)
- `DbSeeder.SeedAsync()` runs after migrations to populate initial data using LINQ queries
- Uses proper EF Core migrations (not `EnsureCreatedAsync()`) for production-ready database management
- All database queries use LINQ Query Syntax for consistency and readability
- Connection string priority:
  1. `DB_CONNECTION_STRING` environment variable
  2. `appsettings.{Environment}.json` (Development uses `appsettings.Development.json`)
  3. `appsettings.json`
  4. Default fallback: `server=db;port=3306;database=crudapp;user=root;password=secret`

**For Local Development:**

- Default connection: `server=localhost;port=3308;database=crudapp;user=root;password=secret`
- Automatically uses Development environment when running `dotnet run`
- Configured via `backend/appsettings.Development.json`

### Manual Migration & Seeding

**Option 1: Create New Migrations (When Models Change)**

When you modify your models (e.g., `Product.cs`), create a new migration:

```bash
# Navigate to API directory
cd backend

# Install EF CLI tool (one-time)
dotnet tool restore

# Create a new migration
dotnet ef migrations add MigrationName

# Example: dotnet ef migrations add AddProductCategory

# Apply migrations manually (optional - they auto-apply on startup)
dotnet ef database update
```

**Note:** Migrations are automatically applied on API startup, but you can apply them manually if needed.

**Reverting Migrations**

Yes, you can revert migrations! Here are the common scenarios:

```bash
# Navigate to API directory
cd backend

# Revert the last migration (removes it from database, keeps migration file)
dotnet ef database update PreviousMigrationName

# Example: If you have migrations: InitialCreate, AddCategory, AddPrice
# To revert to AddCategory:
dotnet ef database update AddCategory

# Revert to the migration before the last one
dotnet ef database update 0  # Reverts to initial state (before any migrations)

# List all migrations to see what's available
dotnet ef migrations list

# Remove the last migration file (if you haven't applied it yet)
dotnet ef migrations remove

# Remove a specific migration (if not applied to database)
# First, manually delete the migration file, then update the snapshot
```

**Important Notes:**

- ⚠️ **Data Loss Warning**: Reverting migrations may cause data loss if the migration removed columns/tables
- **Applied Migrations**: If a migration is already applied to the database, you must use `dotnet ef database update` to revert it
- **Unapplied Migrations**: If a migration hasn't been applied yet, use `dotnet ef migrations remove` to delete it
- **Production**: Be very careful when reverting migrations in production. Always backup your database first!

**Example Workflow:**

```bash
# 1. Check current migration status
dotnet ef migrations list

# 2. See what migrations are applied
# Output shows which migrations are applied (marked with *)

# 3. Revert to a specific migration
dotnet ef database update InitialCreate

# 4. If you want to completely remove a migration file (not yet applied):
dotnet ef migrations remove
```

**Option 2: Run Migrations/Seeder Manually (Docker)**

```bash
# Execute migrations and seeder inside the API container
docker exec -it vuejs-csharp-api dotnet run --project /app

# Or run EF Core commands directly in the container
docker exec -it vuejs-csharp-api bash
# Then inside the container:
cd /app
dotnet ef migrations list
dotnet ef database update
dotnet ef database update PreviousMigrationName  # To revert

# Or connect to database and run SQL manually
docker exec -it vuejs-csharp-db mysql -uroot -psecret crudapp
```

**Option 3: Run Seeder Manually (Local Development)**

```bash
cd backend
dotnet run
# The seeder runs automatically on startup
```

**Option 4: Reset Database and Reseed**

```bash
# Stop containers
docker compose -p crudapp down

# Remove volumes (⚠️ WARNING: Deletes all data)
docker compose -p crudapp down -v

# Start again (will recreate and seed)
docker compose -p crudapp up -d --build
```

**Manual SQL Seeding:**

If you want to seed manually via SQL:

```bash
# Connect to MySQL
docker exec -it vuejs-csharp-db mysql -uroot -psecret crudapp

# Then run:
INSERT INTO Products (Name, Description, Price, Stock) VALUES
('Laptop', '14" Ultrabook', 999.99, 10),
('Mouse', 'Wireless', 24.99, 200),
('Keyboard', 'Mechanical', 79.99, 50);
```

## HTTPS Setup & Nginx Configuration

This project uses **two nginx instances**:

1. **Frontend Nginx** (`client` service): Serves Vue.js static files with HTTPS
2. **Backend Nginx** (`api-nginx` service): Reverse proxy that adds HTTPS to the .NET API

**Quick Overview:**

- **API**: nginx reverse proxy forwards HTTPS requests to the .NET API
- **Client**: nginx serves the Vue.js client with HTTPS
- **SSL Certificates**: Self-signed certificates for development (generated via `nginx/generate-ssl.sh` or `nginx/generate-ssl.ps1`)

**📖 Detailed Nginx Setup Guide**: See [`nginx/NGINX_SETUP.md`](nginx/NGINX_SETUP.md) for complete architecture, configuration, and troubleshooting.

**For production**, replace the self-signed certificates with:

- Let's Encrypt certificates (free, automated)
- Certificates from a trusted CA
- Kubernetes ingress with cert-manager

**Ports (Docker Compose):**

- Client HTTPS: `5443` (redirects from HTTP `5173`)
- API HTTPS: `8443` (redirects from HTTP `8080`)
- MySQL: `3308` (exposed for local development, internal port 3306)
- Adminer: `8081`

**Ports (Local Development):**

- API: `5000` (to avoid conflicts with Docker services)
- Client: `5173`
- MySQL: `3308` (Docker) or `3306` (local MySQL)

## API Endpoints

**Products CRUD:**

- `GET /api/Products` - List products (**cursor-based pagination + filters**)
- `GET /api/Products/{id}` - Get product by ID
- `POST /api/Products` - Create new product
- `PUT /api/Products/{id}` - Update product
- `DELETE /api/Products/{id}` - Delete product

**Auth:**

- `POST /api/Auth/register` - Register (**returns access token** + sets refresh-token cookie + csrf cookie)
- `POST /api/Auth/login` - Login (**returns access token** + sets refresh-token cookie + csrf cookie)
- `POST /api/Auth/refresh` - Get new **access token** (uses refresh-token **HttpOnly cookie** + requires `X-CSRF-TOKEN`)
- `POST /api/Auth/logout` - Revoke refresh token + clear cookies (requires `X-CSRF-TOKEN`)

**JWT secret (Docker Compose):**

Set `JWT_SECRET` for the `api` service (already included in `docker-compose.yml` for development).

**Pagination & Filters (GET /api/Products):**

- **cursor**: last seen `Id` (keyset pagination). Omit for first page.
- **limit**: page size (default 20, max 100)
- **search**: searches `Name` and `Description`
- **minPrice / maxPrice**: price range
- **inStock**: `true` returns only items with `Stock > 0`

Examples:

```bash
# First page
curl -k "https://localhost:8443/api/Products?limit=20"

# Next page (use nextCursor from previous response)
curl -k "https://localhost:8443/api/Products?cursor=42&limit=20"

# Filter + search
curl -k "https://localhost:8443/api/Products?search=mouse&minPrice=10&maxPrice=50&inStock=true&limit=20"
```

**Swagger UI:**

- Docker Compose: https://localhost:8443/swagger
- Kubernetes: http://localhost:30080/swagger
- Local Development: http://localhost:5000/swagger

## Code Implementation

**Database Queries:**

All database queries use **LINQ Query Syntax** for better readability and maintainability:

```csharp
// Example: Get product by ID
var item = await (from product in _db.Products
                 where product.Id == id
                 select product).FirstOrDefaultAsync();
```

**Key Features:**

- ✅ **LINQ Query Syntax**: All queries use explicit `from...where...select` syntax
- ✅ **EF Core Migrations**: Proper migration-based database schema management
- ✅ **Async/Await**: All database operations are asynchronous
- ✅ **AsNoTracking**: Read-only queries use `AsNoTracking()` for better performance
- ✅ **Automatic Seeding**: Database is automatically seeded with sample data on startup

**Project Structure:**

- `backend/Controllers/ProductsController.cs` - REST API endpoints using LINQ queries
- `backend/Data/AppDbContext.cs` - EF Core database context
- `backend/Data/DbSeeder.cs` - Database seeding with LINQ queries
- `backend/Models/Product.cs` - Product entity model
- `backend/Migrations/` - EF Core migration files

## Configuration Files

**Important files for local development:**

- `backend/appsettings.Development.json` - Development connection string (MySQL on port 3308)
- `backend/appsettings.json` - Production/Docker connection string (MySQL on port 3306, server: db)
- `backend/Properties/launchSettings.json` - API port configuration (default: 5000)
- `frontend/src/services/api.js` - Shared Axios client (base URL, auth headers, refresh-token + CSRF handling)
- `frontend/src/services/auth.js` - Frontend auth helpers (in-memory access token, sessionStorage email/CSRF)
- `docker-compose.yml` - Docker services and ports (MySQL on 3308)

## Local Development (Without Docker)

**Requirements:** .NET SDK 10.0, Node.js 18+, MySQL 8.0 (or Docker MySQL on port 3308)

### Setup Database

**Option 1: Use Docker MySQL (Recommended - no local MySQL installation needed)**

```bash
# Start only the MySQL container
docker compose up db -d

# MySQL will be available at localhost:3308
# (Configured to avoid conflicts with local MySQL on port 3306)
```

**Option 2: Use Local MySQL**

If you have MySQL installed locally on port 3306, update `backend/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=crudapp;user=root;password=secret"
  }
}
```

### Run the Application

**1. API:**

```bash
cd backend
dotnet run
# Runs at http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

The API will automatically:

- Use Development environment settings
- Connect to MySQL (localhost:3308 by default, or 3306 if using local MySQL)
- Run database migrations and seed initial data

**2. Client:**

```bash
cd frontend
npm install
npm run dev
# Runs at http://localhost:5173
# Default API URL: http://localhost:5000
```

**Note:** The client is pre-configured to connect to `http://localhost:5000` by default. If you need to change the API URL, set the `VITE_API_URL` environment variable or update the default in `frontend/src/components/Products.vue`.

## Run with Kubernetes (For Production/Scaling)

### 🚀 First Time Deployment

**Step 1: Build images**

```bash
docker compose -p crudapp build api client
```

**Step 2: Build Kubernetes-specific client image**

```bash
docker build -t crudapp-client:k8s -f frontend/Dockerfile.k8s --build-arg VITE_API_URL=http://localhost:30080 .
```

**Step 3: Deploy to Kubernetes**

```bash
kubectl apply -k k8s
```

**Step 4: Wait for pods (watch status)**

```bash
kubectl get pods -n crudapp -w
```

Press `Ctrl+C` to stop watching once all pods show `Running` status.

**Step 5: Access your apps**

- Client: http://localhost:30073
- API Swagger: http://localhost:30080/swagger
- Adminer: http://localhost:30081

---

### ⚡ Redeploy / Update (After first deployment)

**Option 1: Full redeploy** (after code changes)

```bash
# Rebuild images
docker compose -p crudapp build api client
docker build -t crudapp-client:k8s -f frontend/Dockerfile.k8s --build-arg VITE_API_URL=http://localhost:30080 .

# Restart deployments
kubectl rollout restart deployment/api deployment/client -n crudapp

# Check status
kubectl get pods -n crudapp
```

**Option 2: Quick restart** (no code changes, just restart pods)

```bash
kubectl rollout restart deployment/api deployment/client -n crudapp
```

**Option 3: Apply config changes** (if you modified k8s/\*.yaml files)

```bash
kubectl apply -k k8s
```

---

### 🛠️ Useful Commands

```bash
# Status & logs
kubectl get pods -n crudapp
kubectl get services -n crudapp
kubectl logs -n crudapp -l app=api --tail=50
kubectl logs -n crudapp -l app=client --tail=50

# Restart single deployment
kubectl rollout restart deployment/api -n crudapp

# Cleanup
kubectl delete -k k8s
```

---

### 📝 Notes

- Docker Desktop Kubernetes can use local images directly
- For cloud clusters (EKS, GKE, AKS), push images to a registry first

## Troubleshooting

### Port Conflicts

**Issue: Port 8080 already in use**

- **Solution**: The local API now runs on port 5000 by default to avoid conflicts
- If you need to use a different port, update `backend/Properties/launchSettings.json`

**Issue: Port 3306 already in use (local MySQL)**

- **Solution**: Docker MySQL is configured to use port 3308 to avoid conflicts
- Local development uses `localhost:3308` by default
- Update `backend/appsettings.Development.json` if you want to use local MySQL on port 3306

**Issue: Port 5000 already in use**

- **Solution**: Change the port in `backend/Properties/launchSettings.json` and update the client's default API URL in `frontend/src/components/Products.vue`

### Database Connection Issues

**Issue: "Unable to connect to any of the specified MySQL hosts"**

- **Check**: Is MySQL running? (`docker ps` to see if `vuejs-csharp-db` container is running)
- **Check**: Is the port correct? (3308 for Docker, 3306 for local)
- **Check**: Connection string in `appsettings.Development.json`
- **Fix**: Start MySQL with `docker compose up db -d` (wait 10-30 seconds for initialization)
- **Check logs**: `docker logs vuejs-csharp-db` to see if MySQL started successfully

**Issue: API can't connect to database when running locally**

- Ensure MySQL is running (Docker or local)
- Verify connection string in `backend/appsettings.Development.json`
- Check that the Development environment is being used (should see "Environment: Development" in console)

### CORS Issues

**Issue: Client can't connect to API (CORS error)**

- API automatically allows `http://localhost:5173` in Development mode
- Verify API is running on the expected port (default: 5000)
- Check browser console for specific CORS error messages

### Docker Issues

**Issue: Container won't start**

- Check logs: `docker compose -p crudapp logs <service-name>` or `docker logs <container-name>`
- Ensure ports aren't already in use
- Try rebuilding: `docker compose -p crudapp up -d --build`
- Check container status: `docker ps -a` to see all containers (including stopped ones)
- Restart specific container: `docker restart vuejs-csharp-api`

**Issue: Database not seeding or migrations not running**

- Check API logs: `docker logs vuejs-csharp-api` for migration/seeding errors
- Ensure database container is running: `docker ps | grep vuejs-csharp-db`
- Wait 10-30 seconds after starting containers for MySQL to initialize
- Manually trigger seeding by restarting API: `docker restart vuejs-csharp-api`

## Summary

**Docker Compose** = Simple, fast development setup  
**Kubernetes** = Production-ready, scalable deployment  
**Local Development** = Run API and client directly (database via Docker or local MySQL)

**Key Technical Features:**

- ✅ LINQ Query Syntax for all database operations
- ✅ EF Core Migrations for database schema management
- ✅ Automatic database seeding on startup
- ✅ Named Docker containers for easy management
- ✅ HTTPS support with self-signed certificates
- ✅ Full CRUD operations with RESTful API

All methods use the same codebase. Choose based on your needs!

## Backend Tests (Unit / Integration / E2E)

Backend tests live in `backend/Test/` and use **xUnit**.

- **Unit** (`backend/Test/Unit`): tests pure code (mappings, pagination logic) using EF Core **InMemory**
- **Integration** (`backend/Test/Integration`): boots the real API pipeline with `WebApplicationFactory` and swaps MySQL → EF Core **InMemory**
- **E2E (API)** (`backend/Test/E2E`): full CRUD flow over HTTP (POST → GET → PUT → DELETE) using `WebApplicationFactory` + EF Core **InMemory**

### How to run

From the project root:

```bash
cd backend

# Unit
dotnet test .\Test\Unit\Backend.UnitTests.csproj

# Integration
dotnet test .\Test\Integration\Backend.IntegrationTests.csproj

# E2E (API)
dotnet test .\Test\E2E\Backend.E2ETests.csproj
```

## Security Notes (XSS / CSRF)

### Tokens & storage (brief)

- **Access token (JWT)**:
  - **Short-lived** (~15 minutes)
  - Stored **in-memory only** (not persisted) on the frontend
  - Sent on API calls via `Authorization: Bearer <token>`
- **Refresh token**:
  - Stored in **HttpOnly cookie**: `refresh_token` (JavaScript cannot read it)
  - Stored in DB only as a **SHA256 hash** (`RefreshTokens.TokenHash`)
  - **Rotated** on every `/api/Auth/refresh` (old refresh token is revoked, new one is issued)
- **CSRF token** (for cookie-based refresh/logout):
  - Frontend stores a CSRF token in **sessionStorage**
  - Frontend sends it as header `X-CSRF-TOKEN`
  - Backend validates it against **server-side hash** stored with the refresh token (`RefreshTokens.CsrfTokenHash`)
  - CSRF token is **rotated** whenever refresh token rotates

### XSS / CSRF protections

- **XSS**:
  - Vue templates escape output by default (avoid `v-html`)
  - Access token is **not** persisted (reduces impact of XSS persistence)
- **CSRF**:
  - Normal API calls use `Authorization` header (not cookies) → CSRF risk is lower
  - Cookie-based endpoints (`/api/Auth/refresh`, `/api/Auth/logout`) require `X-CSRF-TOKEN` + server-side validation
  - Backend enforces **Origin** checks for refresh/logout

### Production hardening

- **Rate limiting** on auth endpoints (register/login/refresh/logout): `20 req/min/IP`
- **HSTS + HTTPS redirection** enabled when **not** in Development
