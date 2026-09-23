# MarketLink - Pre-Order Fulfillment Activity Diagram

This document illustrates the end-to-end activity and state machine lifecycle for a customer pre-order with cash-on-pickup fulfillment.

---

## Pre-Order Activity & State Machine Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Cart: Customer adds produce to cart
    Cart --> Checkout: Selects Market & Pickup Slot
    Checkout --> Pending: Submits pre-order (Cash on Pickup)
    
    Pending --> Confirmed: Farmer accepts pre-order
    Pending --> Cancelled: Farmer rejects or Customer cancels
    
    Confirmed --> ReadyForPickup: Farmer packages produce & marks ready
    Confirmed --> Cancelled: Out of stock or unexpected issue
    
    ReadyForPickup --> Completed: Customer arrives at market, inspects goods & pays cash
    ReadyForPickup --> Cancelled: Customer no-show after window
    
    Completed --> Reviewed: Customer leaves rating & feedback
    Reviewed --> [*]
    Cancelled --> [*]
```

---

## Detailed Step-by-Step Swimlane Workflow

```mermaid
sequenceDiagram
    autonumber
    actor Customer
    participant MarketLink as MarketLink System
    actor Farmer
    
    Customer->>MarketLink: Browse produce, filter by category/organic
    Customer->>MarketLink: Add produce items to pre-order basket
    Customer->>MarketLink: Select target Market, Pickup Date & Time Slot
    Customer->>MarketLink: Confirm Cash-on-Pickup Pre-Order
    MarketLink->>MarketLink: Decrement provisional inventory / create Order #ORD-XXXX
    MarketLink-->>Farmer: In-app & real-time notification (New Pre-Order)
    
    alt Farmer Accepts
        Farmer->>MarketLink: Review order items & Click "Accept Order"
        MarketLink-->>Customer: Order Status changes to "Confirmed"
        
        Farmer->>MarketLink: Harvest & pack produce -> Click "Ready for Pickup"
        MarketLink-->>Customer: Notification "Your order is ready at [Market] Stall [Stall#]"
        
        Customer->>Farmer: Visits market stall on designated date
        Customer->>Farmer: Verifies items & hands over cash payment
        Farmer->>MarketLink: Click "Complete Order"
        MarketLink-->>Customer: Pre-order marked "Completed"
        Customer->>MarketLink: Submits 5-star rating & review
        Farmer->>MarketLink: Posts thank you response
    else Farmer Rejects / Unfulfilled
        Farmer->>MarketLink: Click "Reject Order" (Reason: Frost/Sold out)
        MarketLink->>MarketLink: Restore product inventory
        MarketLink-->>Customer: Status updated to "Cancelled" with explanation
    end
```
