# Full Stack App (.NET 10 API + Vue 3 Frontend + MySQL)

Full-stack CRUD app: a .NET 10 Web API + a Vue 3 SPA + a MySQL database.
It comes with Docker Compose and Kubernetes manifests so you can run it locally and in a production-like setup.

**Stack:**

- API (backend): .NET 10 Web API, EF Core (LINQ), explicit DTO mappings in `Mappings/`, Pomelo MySQL provider
- Client (frontend): Vue 3 (Vite SPA)
- Database: MySQL 8
- DB Admin: Adminer

## Docker vs Kubernetes (when to use which)

### Docker Compose (best for local dev)

- **Simple**: One command to start everything
- **Fast**: Perfect for local development
- **Easy setup**: No cluster needed

### Kubernetes (good for production/testing)

- **Scalable**: Run multiple replicas, handle traffic spikes
- **Resilient**: Auto-restarts failed pods, health checks
- **Production-ready**: Used by most cloud providers
- **Learning**: Practice real-world deployment

**Quick rule of thumb:**

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

## Quick start (Docker Compose)

### 🚀 First time setup

**1. Build and start**

From the **repo root** (run `npm install` once if you haven't yet):

```bash
npm start
```

Same as `npm run docker:up`. Raw Docker command:

```bash
docker compose -p crudapp up -d --build
```

⏳ Give it **10–30 seconds** on first run (MySQL needs a moment to initialize).

---

### ⚡ Running again

**Just start the services:**

```bash
npm start
```

Or **`npm run docker:start`** (starts without rebuild, faster when images already exist). Raw: `docker compose -p crudapp up -d`.

That’s it.

---

### 🌐 Open the apps

Once everything is up:

- **Client (Vue)**: **http://localhost:5173**
- **API (Swagger)**: **http://localhost:8080/swagger**
- **Adminer (DB UI)**: http://localhost:8081
  - System: MySQL
  - Server: `db` (when using Docker Compose)
  - Username: `root`
  - Password: `secret`
  - Database: `crudapp`

**If you're running the API locally (`dotnet run`):**

- If running API with `dotnet run`, use Adminer with:
  - Server: `host.docker.internal:3308` (when accessing Docker MySQL from Adminer)
  - Or connect directly to local MySQL on `localhost:3306`

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

## Database migrations & seeding

### Quick commands (EF Core)

```bash
# Go to the backend project
cd backend

# Create a migration (after you change models)
dotnet ef migrations add YourMigrationName

# Apply migrations
dotnet ef database update

# Roll back to a specific migration
dotnet ef database update PreviousMigrationName

# Go back to "no migrations" (⚠ resets schema)
dotnet ef database update 0

# Remove the last migration (only if not applied)
dotnet ef migrations remove

# Run the API (applies migrations + seeds on startup)
dotnet run
```

### What happens on startup (default)

When the API starts, it:

1. Applies EF Core migrations (`Database.MigrateAsync()`)
2. Seeds 3 sample products (Laptop, Mouse, Keyboard) if `Products` is empty

**Details:**

- Migrations are applied in `backend/Program.cs`.
- Seeding happens in `backend/Seeder/DbSeeder.cs`. It uses **`CreateProductDto` + `ProductMappings.ToEntity()`** (same mapping path as `POST /api/Products`).
- Reads use `AsNoTracking()` where it makes sense (see `ProductService`, `AuthService`).
- Connection string priority:
  1. `DB_CONNECTION_STRING` environment variable
  2. `appsettings.{Environment}.json` (Development uses `appsettings.Development.json`)
  3. `appsettings.json`
  4. Default fallback: `server=db;port=3306;database=crudapp;user=root;password=secret`

**For Local Development:**

- Default connection: `server=localhost;port=3308;database=crudapp;user=root;password=secret`
- Automatically uses Development environment when running `dotnet run`
- Configured via `backend/appsettings.Development.json`

### Doing it manually (optional)

**Option 1: Create a migration (when models change)**

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

Migrations auto-apply on startup, but manual is handy when you're debugging.

**Rolling back migrations**

You can roll migrations back. Common examples:

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

**Notes:**

- ⚠️ **Data Loss Warning**: Reverting migrations may cause data loss if the migration removed columns/tables
- **Applied Migrations**: If a migration is already applied to the database, you must use `dotnet ef database update` to revert it
- **Unapplied Migrations**: If a migration hasn't been applied yet, use `dotnet ef migrations remove` to delete it
- **Production**: Be very careful when reverting migrations in production. Always backup your database first!

**Example workflow:**

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

**Option 2: Run migrations/seeding via Docker**

```bash
# Run the API inside the container (it will migrate + seed)
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

**Option 3: Run seeding (local dev)**

```bash
cd backend
dotnet run
# Seeder runs automatically on startup
```

**Option 4: Reset DB and reseed**

```bash
# Stop containers
docker compose -p crudapp down

# Remove volumes (⚠️ WARNING: Deletes all data)
docker compose -p crudapp down -v

# Start again (will recreate and seed)
docker compose -p crudapp up -d --build
```

**Manual SQL seed (if you really want to):**

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

## Nginx (HTTP localhost)

This project uses **two nginx instances**:

1. **Frontend Nginx** (`client` service): Serves Vue.js static files on HTTP
2. **Backend Nginx** (`api-nginx` service): Reverse proxy for the .NET API on HTTP

**In plain English:**

- **API**: nginx reverse proxy forwards HTTP requests to the .NET API
- **Client**: nginx serves the Vue.js client on HTTP

**📖 Detailed Nginx Setup Guide**: See [`nginx/NGINX_SETUP.md`](nginx/NGINX_SETUP.md) for complete architecture, configuration, and troubleshooting.

**Ports (Docker Compose):**

- Client HTTP: `5173`
- API HTTP: `8080`
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
curl "http://localhost:8080/api/Products?limit=20"

# Next page (use nextCursor from previous response)
curl "http://localhost:8080/api/Products?cursor=42&limit=20"

# Filter + search
curl "http://localhost:8080/api/Products?search=mouse&minPrice=10&maxPrice=50&inStock=true&limit=20"
```

**Swagger UI:**

- Docker Compose: http://localhost:8080/swagger
- Kubernetes: http://localhost:30080/swagger
- Local Development: http://localhost:5000/swagger

## Code Implementation

### Manual mapping (no AutoMapper)

DTOs and entities are mapped with **explicit C#** in `backend/Mappings/` (easy to grep, debug, and code-review):

| File                          | Role                                                                                                 |
| ----------------------------- | ---------------------------------------------------------------------------------------------------- |
| `Mappings/ProductMappings.cs` | `CreateProductDto` / `UpdateProductDto` → `Product` via `.ToEntity()`                                |
| `Mappings/AuthMappings.cs`    | Email normalization, `RegisterDto` → `User`, `User` + tokens → `AuthResponseDto`, refresh-token rows |

There is **no** AutoMapper, Mapster, or Mapperly dependency—mappings stay visible in source control.

### Request flow (how it runs)

**Create / update product**

1. HTTP `POST` / `PUT` → JSON binds to `CreateProductDto` / `UpdateProductDto`.
2. `ProductsController` calls `dto.ToEntity()` → `Product` (`ProductMappings`).
3. `IProductService` saves with EF Core (`CreateAsync` / `UpdateAsync`).
4. API returns the `Product` JSON (or `204` on update).

**List products (cursor pagination)**

1. `GET /api/Products?cursor=&limit=&search=...` → `ProductQueryParameters`.
2. `ProductService.GetAllAsync` builds an `IQueryable`, applies filters, reads `PagedResult<Product>` (`DTOs/Common/Pagination/PagedResult.cs`).
3. JSON: `{ items, nextCursor, limit }`.

**Register / login / refresh**

1. `RegisterDto` / `LoginDto` → `AuthService` (email normalization via `AuthMappings.NormalizeEmail` where needed).
2. Password hashing and JWT issuance stay in `AuthService`; API responses use `user.ToAuthResponse(accessToken, csrfToken)`.
3. Refresh tokens: `User.ToRefreshToken(...)` builds the `RefreshToken` entity; cookie lifetime follows `AuthMappings.RefreshTokenLifetime`.

**Seed on startup**

- Same as create-product: `DbSeeder` uses `new CreateProductDto { ... }.ToEntity()` so seed data always matches API mapping rules.

### Key features

- ✅ **EF Core Migrations** — schema versioning; `MigrateAsync` on startup
- ✅ **Async I/O** — database calls are asynchronous
- ✅ **AsNoTracking** — used on read-only paths where appropriate
- ✅ **Split queries** — MySQL options use `UseQuerySplittingBehavior(SplitQuery)` to avoid cartesian blowups on multiple `Include`s
- ✅ **Performance-focused methods** — list/read methods are query-shaped (`Where`, `OrderBy`, `Take`, projection) so SQL runs server-side with minimal payload
- ✅ **N+1 prevention** — related data is fetched via projection/controlled includes in single shaped queries (plus split-query safety), avoiding per-row lazy-loading patterns
- ✅ **Read-path indexes** — `Products` has indexes for common filters/pagination (`Price`, `Stock`, and `(Stock, Id)`)
- ✅ **Search optimization** — MySQL FULLTEXT index on (`Name`, `Description`) for faster text search on longer terms
- ✅ **Automatic seeding** — empty `Products` table gets three rows via mapping (see above)

### Performance notes (N+1-safe pattern)

Use query shaping in services so EF Core generates a small number of predictable SQL statements.

```csharp
var query = _context.Products
    .AsNoTracking()
    .Where(p => p.Stock > 0)
    .OrderBy(p => p.Id)
    .Select(p => new ProductListItemDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .Take(limit);

var items = await query.ToListAsync(cancellationToken);
```

Why this helps:

- Projection (`Select`) returns only required columns (smaller payload, less materialization cost).
- Filter + order + limit are pushed to SQL (efficient index-friendly execution).
- One shaped query avoids per-row follow-up queries (prevents classic N+1 behavior).
- Matching indexes (`Price`, `Stock`, `(Stock, Id)`) improve selectivity and keyset pagination scans.
- Search uses `MATCH ... AGAINST` (FULLTEXT) for longer terms, with short-term fallback to `LIKE`.

### Project structure (backend)

- `Controllers/` — HTTP endpoints (`ProductsController`, `AuthController`)
- `Services/` — business logic (`ProductService`, `AuthService`)
- `Mappings/` — DTO ↔ entity / response mapping
- `DTOs/` — `Common/Pagination/`, `Products/`, `Users/`
- `Models/` — EF entities
- `Data/` — `AppDbContext`
- `Seeder/` — `DbSeeder` (startup seed for development)
- `Migrations/` — EF Core migrations

## Configuration Files

**Important files for local development:**

- `backend/Mappings/` - DTO ↔ entity and auth response mapping (`ProductMappings`, `AuthMappings`)
- `backend/appsettings.Development.json` - Development connection string (MySQL on port 3308)
- `backend/appsettings.json` - Production/Docker connection string (MySQL on port 3306, server: db)
- `backend/Properties/launchSettings.json` - API port configuration (default: 5000)
- `frontend/src/services/api.js` - Shared Axios client (base URL, auth headers, refresh-token + CSRF handling)
- `frontend/src/services/auth.js` - Frontend auth helpers (in-memory access token, sessionStorage email/CSRF)
- `frontend/src/main.js` - Vue app bootstrap (Pinia + `@tanstack/vue-query` plugin registration)
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

**Note:** The client is pre-configured to connect to `http://localhost:5000` by default. If you need to change the API URL, set the `VITE_API_URL` environment variable or update the default in `frontend/src/services/api.js`.

**TanStack Query for Vue (React Query equivalent)**

This frontend already uses `@tanstack/vue-query`:

- `frontend/src/main.js` registers `VueQueryPlugin` with a shared `QueryClient`.
- `frontend/src/features/products/composables/useProducts.js` uses query/mutation composables for cached product reads and write invalidation.
- Product/auth flows include normalized error extraction (`detail`, `message`, `title`, string payload, fallback text) to avoid undefined error messages in the UI.

### Error Handling (latest)

- **Backend controllers** (`AuthController`, `ProductsController`) wrap endpoint operations with `try/catch`, log with `ILogger`, and return safe `Problem(...)` responses for unexpected exceptions.
- **Frontend auth/products** paths normalize unknown errors and show fallback messages instead of leaking raw/undefined error objects.
- **App bootstrap/logout** in `frontend/src/App.vue` now handles failures explicitly and surfaces user-friendly alerts.

### Typical run sequence (local)

1. **Database** — MySQL reachable (e.g. `docker compose up db -d` on port **3308**, or local 3306 with updated connection string).
2. **API** — `cd backend && dotnet run` → applies migrations, seeds products if empty, listens (see `launchSettings.json`; often **http://localhost:5000**). Swagger: `/swagger`.
3. **Frontend** — `cd frontend && npm install && npm run dev` → Vite dev server (often **http://localhost:5173**).
4. **Try it** — Register/login via `/api/Auth/*`, then CRUD products. For **how JSON becomes entities** (DTO → mapping → service → DB), see **Code Implementation** → _Manual mapping_ and _Request flow_.

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

- **Solution**: Change the port in `backend/Properties/launchSettings.json` and update the client's default API URL in `frontend/src/services/api.js`

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

**Issue: `client` or `api-nginx` exits immediately (nginx)**

- **Cause (fixed in repo):** Nginx configs must not use `log_format` inside a `server { }` block (only in `http { }`). Invalid nested `location` blocks can also prevent startup.
- **HTTP-only setup:** Client/API nginx run on localhost HTTP ports (`5173` and `8080`) by default.
- **Diagnose:** `docker logs vuejs-csharp-client` or `docker logs vuejs-csharp-api-nginx` — look for `[emerg]` lines. After a rebuild, `nginx -t` should succeed inside the image.

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

- ✅ EF Core with LINQ; manual DTO ↔ entity mapping (`Mappings/`)
- ✅ EF Core Migrations for database schema management
- ✅ Automatic database seeding on startup (via same product mapping as the API)
- ✅ Named Docker containers for easy management
- ✅ HTTP localhost reverse-proxy setup for API and client
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

## Git workflow, commit format & CI

### Git workflow (simple)

```powershell
# 1. See what changed
PS C:\Users\LEGION\Desktop\MyInterviewProject\VueJS-CSharp> git status

# 2. Stage the files you changed
PS C:\Users\LEGION\Desktop\MyInterviewProject\VueJS-CSharp> git add README.md .github/workflows/ci.yml
# or just add everything
PS C:\Users\LEGION\Desktop\MyInterviewProject\VueJS-CSharp> git add .

# 3. Commit with a clear English message
PS C:\Users\LEGION\Desktop\MyInterviewProject\VueJS-CSharp> git commit -m "chore: update README and add CI workflow"

# 4. Push to GitHub (main branch)
PS C:\Users\LEGION\Desktop\MyInterviewProject\VueJS-CSharp> git push origin main
```

### Commit message format

- **Use clear English** that explains the change.
- Try to keep it **at least 10 characters long** so it is descriptive enough.
- A local **Husky + Commitlint** hook will validate the format when you run `git commit`.
- You can follow a lightweight convention like:
  - `feat: add product filters`
  - `fix: handle auth token refresh error`
  - `chore: update README and CI`

To enable Husky/Commitlint locally (one-time per clone):

```bash
cd VueJS-CSharp
npm install
npx husky install
```

### CI workflow (GitHub Actions)

- CI file is at: `.github/workflows/ci.yml`.
- It runs automatically on every **push** or **pull request** to `main`.
- **Backend job**:
  - Restores and builds the .NET 10 API.
  - Checks C# code format with `dotnet format --verify-no-changes`.
  - Runs Unit, Integration, and E2E tests under `backend/Test`.
- **Frontend job**:
  - Installs dependencies in `frontend` with `npm ci`.
  - Runs frontend tests with `npm test` (Vitest).

You can see CI results in the **Actions** tab of the GitHub repo.
