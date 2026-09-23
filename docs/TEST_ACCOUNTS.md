# MarketLink - Seeded Test Credentials

MarketLink automatically seeds test accounts on initial launch through `DatabaseSeeder.cs`. You can use these accounts to verify each persona's role-based features.

---

## 1. System Administrator

| Role | Email | Password | Full Name | Primary Capabilities |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin@marketlink.com` | `Admin@123456` | System Administrator | Dashboard analytics, approve/reject farmers, manage markets & categories, moderate reviews, system reports |

- **Admin Area Access**: `/Admin` or `/Admin/Dashboard`

---

## 2. Farmer Vendors

All farmer accounts have password: **`Farmer@123456`**

| Email | Farm Name | Stall | Specialty | Featured |
| :--- | :--- | :---: | :--- | :---: |
| `john@greenfarms.com` | Harrison Organic Farms | A1 | Heirloom Vegetables, Greens | Yes |
| `sarah@sunnyside.com` | Sunnyside Dairy Farm | B3 | Artisan Cheese, Yogurt, Milk | Yes |
| `miguel@miguelsfarm.com` | Miguel's Heritage Farm | C7 | Traditional Organic Fruits & Roots | Yes |
| `emma@herbgarden.com` | Emma's Herb Garden | D2 | Culinary & Medicinal Fresh Herbs | No |
| `david@riverbanks.com` | Riverbanks Fresh Produce | E5 | Seasonal Berries & Orchard Fruits | No |

- **Farmer Area Access**: `/Farmer` or `/Farmer/Dashboard`
- **Farmer Features**: Add/edit produce, adjust inventory, set pickup time slots, accept/reject pre-orders, mark ready for pickup, inspect sales revenue analytics.

---

## 3. Customer Shoppers

All customer accounts have password: **`Customer@123456`**

| Email | Name | City | Sample Data |
| :--- | :--- | :--- | :--- |
| `alice@example.com` | Alice Johnson | Springfield | Has saved favorites, default delivery address, pre-order history |
| `bob@example.com` | Bob Williams | Springfield | Active customer with cart items |
| `carol@example.com` | Carol Davis | Chicago | Pre-order history |

- **Customer Dashboard Access**: `/Customer` or `/Customer/Dashboard`
- **Customer Features**: Search & filter produce, interactive market map, cash-on-pickup pre-orders, saved favorites, 1-click reorder, reviews & ratings, multi-address management.
