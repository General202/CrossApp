namespace Core.Reports;

// DTO для рядка звіту замовлення
public sealed record OrderReportLine(
    string OrderId,
    string CustomerId,
    string Status,
    decimal TotalAmount,
    int LinesCount);

// DTO для підсумку групи (наприклад, за статусом)
public sealed record GroupSummary(
    string Key,
    int Count,
    decimal TotalAmount);

// DTO для об'єднаного звіту Клієнт-Замовлення (Додаткове завдання 1)
public sealed record CustomerSummaryReport(
    string CustomerId,
    int TotalOrders,
    decimal TotalSpent);