namespace MarketLink.Core.Enums;

public enum UserRole
{
    Admin,
    Farmer,
    Customer
}

public enum OrderStatus
{
    Pending,
    Accepted,
    Processing,
    ReadyForPickup,
    Completed,
    Cancelled
}

public enum FarmerStatus
{
    Pending,
    Approved,
    Suspended,
    Rejected
}

public enum NotificationType
{
    OrderPlaced,
    OrderAccepted,
    OrderReady,
    OrderCancelled,
    OrderCompleted,
    NewReview,
    LowStock,
    FarmerApproved,
    FarmerSuspended,
    General
}

public enum ReportType
{
    SalesReport,
    InventoryReport,
    FarmerPerformance,
    CustomerActivity,
    PlatformOverview
}

public enum Season
{
    AllYear,
    Spring,
    Summer,
    Autumn,
    Winter
}
