# MarketLink - Database Entity-Relationship (ER) Diagram

This document details the complete relational database schema for **MarketLink**, highlighting entities, primary keys, foreign keys, relationships, and cardinalities.

## Database Technology
- **RDBMS**: Microsoft SQL Server
- **ORM**: Entity Framework Core 8 / 10
- **Data Model**: Code-First with migrations

---

## Entity-Relationship Diagram (Mermaid)

```mermaid
erDiagram
    AspNetUsers ||--o| Farmers : "has one"
    AspNetUsers ||--o| Customers : "has one"
    AspNetUsers ||--o{ Notifications : "receives"
    
    Customers ||--o{ CustomerAddresses : "has many"
    Customers ||--o{ Orders : "places"
    Customers ||--o{ Favorites : "saves"
    Customers ||--o{ Reviews : "writes"
    Customers ||--o{ CartItems : "has"

    Farmers ||--o{ Products : "sells"
    Farmers ||--o{ PickupSlots : "defines"
    Farmers ||--o{ Orders : "fulfills"
    Farmers ||--o{ Favorites : "bookmarked by"

    Markets ||--o{ PickupSlots : "hosts"
    Markets ||--o{ Orders : "pickup location"
    Markets ||--o{ MarketFarmers : "stall holders"
    Farmers ||--o{ MarketFarmers : "participates in"

    Categories ||--o{ Categories : "sub-categories"
    Categories ||--o{ Products : "categorizes"

    Products ||--o{ OrderItems : "ordered in"
    Products ||--o{ CartItems : "in cart"
    Products ||--o{ Favorites : "saved as"
    Products ||--o{ Reviews : "reviewed in"

    Orders ||--o{ OrderItems : "contains"
    Orders ||--o{ OrderStatusHistories : "tracks"
    Orders }o--|| PickupSlots : "scheduled at"
    Orders }o--|| Markets : "collected from"

    AspNetUsers {
        string Id PK
        string UserName
        string Email
        string PhoneNumber
        string FirstName
        string LastName
        string ProfileImageUrl
        datetime CreatedAt
        datetime UpdatedAt
        bit IsActive
    }

    Customers {
        int Id PK
        string UserId FK
        string Bio
        string DefaultCity
    }

    CustomerAddresses {
        int Id PK
        int CustomerId FK
        string Label
        string AddressLine1
        string AddressLine2
        string City
        string State
        string PostalCode
        bit IsDefault
    }

    Farmers {
        int Id PK
        string UserId FK
        string FarmName
        string Description
        string StallNumber
        string BannerImageUrl
        string LogoUrl
        int Status
        bit IsFeatured
        datetime RegisteredAt
        datetime ApprovedAt
    }

    Markets {
        int Id PK
        string Name
        string Description
        string Address
        string City
        string State
        string PostalCode
        float Latitude
        float Longitude
        string OperatingHours
        string OpenDays
        string ImageUrl
        bit IsActive
        bit IsFeatured
    }

    MarketFarmers {
        int Id PK
        int MarketId FK
        int FarmerId FK
        string StallNumber
        bit IsActive
    }

    Categories {
        int Id PK
        string Name
        string Slug
        string Description
        string IconClass
        string ImageUrl
        int ParentId FK
        int SortOrder
        bit IsActive
    }

    Products {
        int Id PK
        int FarmerId FK
        int CategoryId FK
        string Name
        string Slug
        string Description
        decimal Price
        string Unit
        int StockQuantity
        bit IsOrganic
        int Season
        string ImageUrl
        bit IsActive
        bit IsFeatured
        datetime CreatedAt
        datetime UpdatedAt
    }

    PickupSlots {
        int Id PK
        int FarmerId FK
        int MarketId FK
        int DayOfWeek
        time StartTime
        time EndTime
        int MaxOrders
        bit IsActive
    }

    Orders {
        int Id PK
        string OrderNumber
        int CustomerId FK
        int FarmerId FK
        int MarketId FK
        int PickupSlotId FK
        datetime PickupDate
        int Status
        decimal TotalAmount
        string CustomerNotes
        string CancellationReason
        datetime CreatedAt
        datetime UpdatedAt
    }

    OrderItems {
        int Id PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal TotalPrice
        string ProductName
        string Unit
    }

    OrderStatusHistories {
        int Id PK
        int OrderId FK
        int FromStatus
        int ToStatus
        string ChangedBy
        string Note
        datetime ChangedAt
    }

    Reviews {
        int Id PK
        int CustomerId FK
        int ProductId FK
        int FarmerId FK
        int Rating
        string Comment
        string FarmerResponse
        datetime RespondedAt
        bit IsApproved
        datetime CreatedAt
    }

    Favorites {
        int Id PK
        int CustomerId FK
        int ProductId FK
        int FarmerId FK
        datetime CreatedAt
    }

    CartItems {
        int Id PK
        int CustomerId FK
        int ProductId FK
        int Quantity
        datetime CreatedAt
    }

    Notifications {
        int Id PK
        string UserId FK
        string Title
        string Message
        int Type
        string TargetUrl
        bit IsRead
        datetime CreatedAt
    }

    SiteSettings {
        int Id PK
        string Key
        string Value
        string Group
        string Description
        datetime UpdatedAt
    }
```

---

## Entity Specifications

| Table | Description | Primary Key | Key Foreign Keys |
| :--- | :--- | :--- | :--- |
| `AspNetUsers` | ASP.NET Identity user accounts | `Id (nvarchar 450)` | - |
| `Customers` | Profile data for shoppers | `Id (int)` | `UserId` -> `AspNetUsers.Id` |
| `CustomerAddresses` | Multi-address book (Home, Work, Other) | `Id (int)` | `CustomerId` -> `Customers.Id` |
| `Farmers` | Approved farm vendor profiles | `Id (int)` | `UserId` -> `AspNetUsers.Id` |
| `Markets` | Physical farmer market locations with lat/long | `Id (int)` | - |
| `MarketFarmers` | Association of farmers and market stalls | `Id (int)` | `MarketId`, `FarmerId` |
| `Categories` | Hierarchical produce categorization | `Id (int)` | `ParentId` -> `Categories.Id` |
| `Products` | Produce catalog with price, unit, stock | `Id (int)` | `FarmerId`, `CategoryId` |
| `PickupSlots` | Reserved pickup time windows | `Id (int)` | `FarmerId`, `MarketId` |
| `Orders` | Customer pre-orders with cash-on-pickup | `Id (int)` | `CustomerId`, `FarmerId`, `MarketId`, `PickupSlotId` |
| `OrderItems` | Line items snapshot for each order | `Id (int)` | `OrderId`, `ProductId` |
| `OrderStatusHistories` | Audit trail of order status transitions | `Id (int)` | `OrderId` -> `Orders.Id` |
| `Reviews` | Customer ratings & feedback with farmer replies | `Id (int)` | `CustomerId`, `ProductId`, `FarmerId` |
| `Favorites` | Bookmarked products and farms | `Id (int)` | `CustomerId`, `ProductId`, `FarmerId` |
| `CartItems` | Persistent pre-order shopping cart | `Id (int)` | `CustomerId`, `ProductId` |
| `Notifications` | Real-time & in-app alerts for users | `Id (int)` | `UserId` -> `AspNetUsers.Id` |
| `SiteSettings` | Dynamic system and portal configurations | `Id (int)` | - |
