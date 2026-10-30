using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "orders.json");
IOrderStore store = StoreFactory.CreateStore(args, jsonPath);
var service = new OrderService(store);

// -------------------------------------------------------------------
// 1. Демонстрація відкладеного виконання (Deferred Execution)
// -------------------------------------------------------------------
if (args.Contains("demo-deferred"))
{
    Console.WriteLine("==========================================================");
    Console.WriteLine("   ДЕМОНСТРАЦІЯ ВІДКЛАДЕНОГО ВИКОНАННЯ (DEFERRED EXECUTION)");
    Console.WriteLine("==========================================================");

    // Створюємо мок-список для чистоти ефекту
    var sourceList = SampleData.GetOrders();

    // 1. Запит оголошено, але ще НЕ виконано!
    var deferredQuery = sourceList.Where(o => o.Status == OrderStatus.Draft);
    Console.WriteLine($"1. Початкова кількість чернеток (query.Count()): {deferredQuery.Count()}");

    // 2. Змінюємо джерело ПІСЛЯ створення запиту
    var newDraft = Order.Create("ORD-TMP-999", "CUST-999");
    sourceList.Add(newDraft);
    Console.WriteLine($"2. Кількість чернеток після додавання в джерело (query.Count()): {deferredQuery.Count()}");

    // 3. Фіксуємо знімок через ToList()
    var snapshot = deferredQuery.ToList();
    sourceList.Clear(); // Очищаємо джерело
    Console.WriteLine($"3. Кількість чернеток у матеріалізованому знімку (snapshot.Count): {snapshot.Count}");
    Console.WriteLine($"4. Кількість у початковому запиті після Clear (query.Count()): {deferredQuery.Count()}");
    return 0;
}

// -------------------------------------------------------------------
// 2. Обробка підкоманд звітів через switch pattern matching
// -------------------------------------------------------------------
var items = service.GetAllOrders();
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

Console.WriteLine("==========================================================");
Console.WriteLine($"   ВИКОНАННЯ КОМАНДИ ЗВІТУ: {command.ToUpper()}");
Console.WriteLine("==========================================================");

switch (command)
{
    case "report-top":
        PrintLines(ReportService.TopByAmount(items, 5));
        break;

    case "report-low":
        Console.WriteLine("Замовлення на малу суму (< 2000 грн):");
        PrintLines(ReportService.FilterByMinAmount(items.Where(o => o.TotalAmount < 2000m), 0m));
        break;

    case "report-groups":
        Console.WriteLine($"{"Статус",-12} | {"Кількість",10} | {"Загальна Сума",15}");
        Console.WriteLine(new string('-', 45));
        foreach (var g in ReportService.SummaryByStatus(items))
        {
            Console.WriteLine($"{g.Key,-12} | {g.Count,10} | {g.TotalAmount,15:F2} грн");
        }
        break;

    case "report-sum":
        var t = ReportService.OverallTotals(items);
        Console.WriteLine($"Всього замовлень : {t.Count}");
        Console.WriteLine($"Загальна сума   : {t.Total:F2} грн");
        Console.WriteLine($"Середній чек    : {t.Average:F2} грн");
        Console.WriteLine($"Максимальне чек : {t.Max:F2} грн");
        break;

    case "report-benchmark": // Додаткове завдання 4
        Console.WriteLine("Запуск бенчмарку LINQ на 100 000 елементів...");
        var bench = ReportService.BenchmarkQueryPerformance(100_000);
        Console.WriteLine($"Час з ToList() всередині ланцюжка : {bench.TimeWithMidToListMs} ms");
        Console.WriteLine($"Час чистого LINQ (без ToList)     : {bench.TimePureQueryMs} ms");
        break;

    case "report-join": // Додаткове завдання 1
        var custIds = new[] { "CUST-001", "CUST-002", "CUST-003", "CUST-004", "CUST-999" };
        var custReport = ReportService.CustomerSummaryGroupJoin(items, custIds);
        Console.WriteLine($"{"Клієнт",-10} | {"Замовлень",10} | {"Загалом Витрачено",18}");
        Console.WriteLine(new string('-', 45));
        foreach (var c in custReport)
        {
            Console.WriteLine($"{c.CustomerId,-10} | {c.TotalOrders,10} | {c.TotalSpent,18:F2} грн");
        }
        break;

    default:
        Console.WriteLine("Доступні команди звітів:");
        Console.WriteLine("  report-top       - Топ-5 замовлень за сумою");
        Console.WriteLine("  report-low       - Замовлення на малу суму");
        Console.WriteLine("  report-groups    - Зведення за статусами (GroupBy)");
        Console.WriteLine("  report-sum       - Загальні агрегати (Sum/Average/Max)");
        Console.WriteLine("  report-join      - Зведений звіт Клієнти-Замовлення (GroupJoin)");
        Console.WriteLine("  report-benchmark - Порівняння продуктивності LINQ");
        Console.WriteLine("  demo-deferred    - Демонстрація відкладеного виконання");
        break;
}

return 0;

static void PrintLines(IReadOnlyList<OrderReportLine> lines)
{
    Console.WriteLine($"{"ID Замовлення",-14} | {"Клієнт",-10} | {"Статус",-10} | {"Позицій",8} | {"Сума",12}");
    Console.WriteLine(new string('-', 65));
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.OrderId,-14} | {l.CustomerId,-10} | {l.Status,-10} | {l.LinesCount,8} | {l.TotalAmount,12:F2} грн");
    }
}