# MarketLink - REST API Documentation

MarketLink provides a comprehensive, production-grade REST API surface designed for mobile applications, third-party integrations, and headless clients.

Interactive OpenAPI documentation is available locally at:
**`/swagger`** (e.g. `https://localhost:7198/swagger` or `http://localhost:5000/swagger`).

---

## 1. Authentication APIs (`/api/auth`)

### `POST /api/auth/login`
Authenticates a user and generates a cookie session / user profile response.

- **Request Body**:
```json
{
  "email": "alice@example.com",
  "password": "Customer@123456"
}
```

- **Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login successful",
  "user": {
    "id": "guid",
    "email": "alice@example.com",
    "fullName": "Alice Johnson",
    "roles": ["Customer"]
  }
}
```

### `POST /api/auth/register`
Creates a new customer or farmer account.

- **Request Body**:
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "johndoe@example.com",
  "password": "Password@123",
  "role": "Customer"
}
```

### `GET /api/auth/me`
Retrieves currently logged-in user profile, role, and details.

---

## 2. Products APIs (`/api/products`)

### `GET /api/products`
Retrieves paginated, filtered produce listings.

- **Query Parameters**:
  - `search` (string): Search keywords (matches product name and description).
  - `categoryId` (int): Filter by specific produce category.
  - `isOrganic` (bool): Filter only certified organic produce.
  - `season` (int): Season filter (0=Spring, 1=Summer, 2=Autumn, 3=Winter, 4=YearRound).
  - `sortBy` (string): `price_asc`, `price_desc`, `newest`, `popular`.
  - `page` (int, default: 1): Page number.
  - `pageSize` (int, default: 12): Items per page.

- **Response (200 OK)**:
```json
{
  "items": [
    {
      "id": 1,
      "name": "Heirloom Beefsteak Tomatoes",
      "slug": "heirloom-beefsteak-tomatoes",
      "price": 4.50,
      "unit": "kg",
      "stockQuantity": 45,
      "isOrganic": true,
      "season": "Summer",
      "farmName": "Harrison Organic Farms",
      "imageUrl": "/images/products/tomatoes.jpg"
    }
  ],
  "totalCount": 38,
  "page": 1,
  "pageSize": 12,
  "totalPages": 4
}
```

### `GET /api/products/{id}`
Returns complete product details with farmer stall info, stock status, and recent reviews.

---

## 3. Categories APIs (`/api/categories`)

### `GET /api/categories`
Returns all active categories and their nested sub-categories.

---

## 4. Markets APIs (`/api/markets`)

### `GET /api/markets`
Retrieves physical farmers markets with geographic coordinates (latitude, longitude), active days, and operating hours.

- **Response (200 OK)**:
```json
[
  {
    "id": 1,
    "name": "Central Farmers Market",
    "address": "100 Market Square",
    "city": "Springfield",
    "state": "IL",
    "latitude": 39.7817,
    "longitude": -89.6501,
    "operatingHours": "8:00 AM - 1:00 PM",
    "openDays": "Saturday,Sunday",
    "isFeatured": true
  }
]
```

### `GET /api/markets/{id}/farmers`
Returns the list of farmers and stalls operating at this specific market.

---

## 5. Orders APIs (`/api/orders`)

### `POST /api/orders`
Places a new cash-on-pickup pre-order.

- **Request Body**:
```json
{
  "farmerId": 1,
  "marketId": 1,
  "pickupSlotId": 2,
  "pickupDate": "2026-09-26",
  "customerNotes": "Please pack in paper bags if possible.",
  "items": [
    { "productId": 1, "quantity": 3 },
    { "productId": 4, "quantity": 1 }
  ]
}
```

### `GET /api/orders/my-orders`
Returns the authenticated customer's pre-order history and statuses.

### `PATCH /api/orders/{id}/status`
*(Farmer/Admin only)* Updates order status: `Accept`, `Reject`, `MarkReady`, `Complete`, `Cancel`.

---

## 6. Favorites APIs (`/api/favorites`)

### `GET /api/favorites`
Lists all bookmarked products and farms for the authenticated customer.

### `POST /api/favorites/toggle-product/{productId}`
Toggles product bookmark on/off.

### `POST /api/favorites/toggle-farmer/{farmerId}`
Toggles farmer bookmark on/off.

---

## 7. Reviews APIs (`/api/reviews`)

### `POST /api/reviews`
Submits a star rating (1-5) and feedback comment for a product and farmer.

---

## 8. Notifications APIs (`/api/notifications`)

### `GET /api/notifications`
Fetches user notifications (unread first).

### `POST /api/notifications/{id}/read`
Marks a notification as read.
