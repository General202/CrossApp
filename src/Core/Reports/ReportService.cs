using System.Diagnostics;
using Core.Domain;

namespace Core.Reports;

public static class ReportService
{
    // 1) Топ-N найдорожчих замовлень із гарантовано стабільним сортуванням (OrderByDescending + ThenBy)
    public static IReadOnlyList<OrderReportLine> TopByAmount(IEnumerable<Order> source, int n)
    {
        return source
            .OrderByDescending(o => o.TotalAmount)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Take(n)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Status.ToString(), o.TotalAmount, o.Lines.Count))
            .ToList();
    }

    // 2) Фільтрація + сортування + проєкція в DTO (Замовлення з сумою >= threshold)
    public static IReadOnlyList<OrderReportLine> FilterByMinAmount(IEnumerable<Order> source, decimal minAmount)
    {
        return source
            .Where(o => o.TotalAmount >= minAmount)
            .OrderBy(o => o.TotalAmount)
            .ThenBy(o => o.Id, StringComparer.Ordinal)
            .Select(o => new OrderReportLine(o.Id, o.CustomerId, o.Status.ToString(), o.TotalAmount, o.Lines.Count))
            .ToList();
    }

    // 3) Групування за статусом із підрахунком кількості та суми
    public static IReadOnlyList<GroupSummary> SummaryByStatus(IEnumerable<Order> source)
    {
        return source
            .GroupBy(o => o.Status.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(o => o.TotalAmount)))
            .OrderByDescending(g => g.TotalAmount)
            .ToList();
    }

    // 4) Агрегати з безпечною обробкою порожньої послідовності
    public static (int Count, decimal Total, double Average, decimal Max) OverallTotals(IEnumerable<Order> source)
    {
        var list = source.ToList();
        if (list.Count == 0)
        {
            return (0, 0m, 0d, 0m);
        }

        return (
            list.Count,
            list.Sum(o => o.TotalAmount),
            (double)list.Average(o => o.TotalAmount),
            list.Max(o => o.TotalAmount)
        );
    }

    // -------------------------------------------------------------------
    // ДОДАТКОВІ ЗАВДАННЯ (1, 3, 4)
    // -------------------------------------------------------------------

    // Додаткове завдання 1: GroupJoin Клієнти + Замовлення
    public static IReadOnlyList<CustomerSummaryReport> CustomerSummaryGroupJoin(IEnumerable<Order> source, IEnumerable<string> customerIds)
    {
        return customerIds.GroupJoin(
            source,
            custId => custId,
            order => order.CustomerId,
            (custId, orders) => new CustomerSummaryReport(
                custId,
                orders.Count(),
                orders.Sum(o => o.TotalAmount)
            )
        ).OrderByDescending(c => c.TotalSpent).ToList();
    }

    // Додаткове завдання 3: Запис звіту у синтаксисі запитів (Query Syntax: from ... group ... into)
    public static IReadOnlyList<GroupSummary> SummaryByStatusQuerySyntax(IEnumerable<Order> source)
    {
        var query = from order in source
                    group order by order.Status.ToString() into g
                    orderby g.Sum(o => o.TotalAmount) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(o => o.TotalAmount));

        return query.ToList();
    }

    // Додаткове завдання 4: Бенчмарк Stopwatch на 100 000 замовлень (з ToList всередині ланцюжка vs без нього)
    public static (long TimeWithMidToListMs, long TimePureQueryMs) BenchmarkQueryPerformance(int count = 100_000)
    {
        // Генерація тестових 100,000 об'єктів у пам'яті
        var generated = Enumerable.Range(1, count)
            .Select(i => Order.Create($"ORD-BENCH-{i}", $"CUST-{i % 50}"))
            .ToList();

        // 1. З ToList() посередині (передчасна матеріалізація)
        var sw1 = Stopwatch.StartNew();
        var res1 = generated
            .Where(o => o.Id.Length > 0)
            .ToList() // ❌ зайве збереження 100 000 елементів у новий список
            .Where(o => o.CustomerId.StartsWith("CUST-1"))
            .Select(o => o.Id)
            .ToList();
        sw1.Stop();

        // 2. Чистий LINQ ланцюжок (без передчасного ToList)
        var sw2 = Stopwatch.StartNew();
        var res2 = generated
            .Where(o => o.Id.Length > 0)
            .Where(o => o.CustomerId.StartsWith("CUST-1"))
            .Select(o => o.Id)
            .ToList();
        sw2.Stop();

        return (sw1.ElapsedMilliseconds, sw2.ElapsedMilliseconds);
    }
}