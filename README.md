# MarketLink – Farmers Marketplace (eGreen Basket)

> A modern, production-grade web application connecting local farmers with community shoppers for farm-fresh produce and scheduled cash-on-pickup pre-orders.

Built with **ASP.NET Core 8 MVC**, **Entity Framework Core**, **SQL Server**, **Bootstrap 5**, **Clean Architecture**, **Repository & Unit of Work Patterns**, and **REST APIs with OpenAPI/Swagger**.

---

## 🌟 Key Platform Features

### 🛒 Customer Experience
- **Public Produce Catalog**: Advanced search, multi-faceted filtering (categories, certified organic, season, price range, sorting).
- **Physical Markets with Map Coordinates**: Discover local farmers markets with operating days, timings, and interactive location views.
- **Cash-on-Pickup Pre-Orders**: Build a cart, select specific pickup dates and farmer time windows, with physical cash payment at pickup (no online gateway needed).
- **Customer Dashboard**:
  - Saved favorites synchronized between navbar and dashboard.
  - Multi-address management (Home, Work, Other) with default selector.
  - Complete order history with status tracking and 1-click reordering.
  - Product & farmer review submissions with 5-star ratings.
  - In-app notification center.
  - Responsive collapsible sidebar with fixed brand header.

### 🚜 Farmer Vendor Module
- **Farmer Dashboard**: KPI cards for active orders, today's pickups, low stock alerts, and monthly earnings.
- **Produce Management**: Full CRUD for produce items with units (kg, lb, bunch, dozen), pricing, stock tracking, and organic badges.
- **Pickup Slots & Capacity**: Schedule recurring weekly market stalls and maximum pre-order capacities.
- **Order Processing Workflow**: Review incoming pre-orders, Accept or Reject with reason, mark Ready for Pickup, and complete handoff.
- **Sales Analytics**: Visual sales trends, revenue summaries, and top-selling produce.
- **Customer Feedback**: Review customer ratings and post direct responses.

### 🛡️ Administrator Module
- **System Dashboard**: High-level platform KPIs, total orders, transaction volume, and recent audit logs.
- **Farmer Moderation**: Review vendor applications, approve, reject, or suspend farmer accounts.
- **Market & Stall Management**: Add physical market venues with GPS coordinates, hours, and open days.
- **Category Tree Management**: Maintain main produce categories and nested sub-categories with display order and icons.
- **Customer Oversight**: Inspect customer activity and toggle account statuses.
- **Audit & Review Moderation**: Moderate customer reviews and remove inappropriate content.
- **System Reports**: Exportable analytics on pre-orders, revenue, and farmer performance.

### 🌐 REST API & OpenAPI / Swagger
- Fully featured REST endpoints under `/api/*` for mobile apps and integrations.
- Interactive documentation at **`/swagger`**.

---

## 🏛️ Architecture & Project Structure

MarketLink follows the **Clean Architecture** principle:

```
MarketLink-User-Side/
│
├── MarketLink.Core/                 # Domain Layer
│   ├── Entities/                    # Product, Farmer, Customer, Order, Market, Review, etc.
│   ├── Enums/                       # OrderStatus, FarmerStatus, Season, NotificationType
│   ├── Interfaces/                  # IRepository, IUnitOfWork, IProductService, IOrderService, etc.
│   └── ViewModels/                  # DTOs and Presentation Models
│
├── MarketLink.Infrastructure/       # Data & External Concerns
│   ├── Data/                        # ApplicationDbContext, Entity Configurations
│   │   └── SeedData/                # DatabaseSeeder (Accounts, Categories, Markets, Produce)
│   ├── Repositories/                # Generic Repository<T>, UnitOfWork
│   └── Services/                    # Domain Service Implementations (ProductService, CartService, etc.)
│
├── MarketLink.Web/                  # Presentation Layer
│   ├── Areas/
│   │   ├── Admin/                   # Dashboard, Farmers, Customers, Markets, Categories, Reports
│   │   ├── Farmer/                  # Dashboard, Products, Orders, Pickup Slots, Analytics, Profile
│   │   └── Customer/                # Dashboard, Orders, Favorites, Profile, Addresses, Cart
│   ├── Controllers/                 # Public MVC Controllers (Home, Products, Markets, Auth)
│   │   └── Api/                     # REST API Controllers (Auth, Products, Orders, Markets, etc.)
│   ├── Views/                       # Responsive Razor Views with Custom Modern Styling
│   └── wwwroot/                     # Custom CSS, JS, Vendor Libraries, Icons, and Media
│
└── docs/                            # Architectural Documentation & UML Diagrams
    ├── DATABASE_ER_DIAGRAM.md       # Entity-Relationship diagram (Mermaid)
    ├── USE_CASE_DIAGRAM.md          # Use Case diagram & actor matrix
    ├── CLASS_DIAGRAM.md             # Class hierarchy & Clean Architecture
    ├── ACTIVITY_DIAGRAM.md          # Pre-order lifecycle sequence & state machine
    ├── API_DOCUMENTATION.md         # REST API endpoints & request/response schemas
    ├── DEPLOYMENT_GUIDE.md          # IIS, Azure, SmarterASP, and Docker guides
    └── TEST_ACCOUNTS.md             # Pre-configured test credentials
```

---

## 🚀 Quick Start Guide

### 1. Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher.
- [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB, Express, or full instance).

### 2. Configure Connection String
Open `MarketLink.Web/appsettings.json` and set your SQL Server connection:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MarketLinkDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 3. Build & Run
From the root solution directory:

```bash
dotnet restore
dotnet build
cd MarketLink.Web
dotnet run
```

The database will be automatically created and populated with demo categories, markets, products, and user accounts on initial startup via Entity Framework Core migrations and `DatabaseSeeder`.

Open your browser at `https://localhost:7198` (or the console URL).

---

## 🔑 Demo Accounts

Refer to [`docs/TEST_ACCOUNTS.md`](docs/TEST_ACCOUNTS.md) for full credentials.

| Role | Email | Password | Access Area |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@marketlink.com` | `Admin@123456` | `/Admin` |
| **Farmer** | `john@greenfarms.com` | `Farmer@123456` | `/Farmer` |
| **Customer** | `alice@example.com` | `Customer@123456` | `/Customer` |

---

## 📚 Technical Documentation & Diagrams
- **[Database ER Diagram](docs/DATABASE_ER_DIAGRAM.md)**
- **[Use Case Diagram](docs/USE_CASE_DIAGRAM.md)**
- **[Class & Architecture Diagram](docs/CLASS_DIAGRAM.md)**
- **[Activity & Fulfillment Sequence](docs/ACTIVITY_DIAGRAM.md)**
- **[REST API Specifications](docs/API_DOCUMENTATION.md)**
- **[Production Deployment Guide](docs/DEPLOYMENT_GUIDE.md)**
- **[Test Accounts Reference](docs/TEST_ACCOUNTS.md)**

---

## 📱 Responsive Design Verification
All views in the application (Public Catalog, Checkout, Customer Dashboard, Farmer Console, and Admin Portal) are fully tested and responsive across:
- **Mobile (375px - 576px)**: Adaptive offcanvas menus, single-column touch cards, sticky bottom action bars.
- **Tablet (768px - 991px)**: Dual-column grid layouts and collapsible sidebars.
- **Desktop (1200px+)**: Multi-column data tables, rich KPI visual cards, and side-by-side management views.
