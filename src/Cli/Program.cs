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

// Налаштування кооперативного скасування через Ctrl+C
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    Console.WriteLine("\n[CTRL+C] Отримано сигнал зупинки! Скасовуємо операцію...");
    e.Cancel = true;
    cts.Cancel();
};

// Перевірка додаткових прапорців
if (args.Contains("--timeout"))
{
    cts.CancelAfter(TimeSpan.FromMilliseconds(50)); // Швидкий таймаут для тесту скасування
}

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "import";

try
{
    if (command == "import")
    {
        Console.WriteLine("--- 1. Асинхронний імпорт замовлень із перевіркою скасування ---");

        var importer = new JsonOrderImporter();
        var progress = new Progress<int>(pct => Console.Write($"\rПрогрес імпорту: {pct}%"));

        int imported = await service.ImportOrdersAsync(importer, jsonPath, progress, cts.Token);
        Console.WriteLine($"\nУспішно імпортовано нових замовлень: {imported}");
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n [СКАСОВАНО] Операцію імпорту було скасовано користувачем або за таймаутом.");
}
catch (DomainException ex)
{
    Console.WriteLine($"\n [ДОМЕННА ПОМИЛКА] Дані відхилено: {ex.Message}");
}
catch (FileNotFoundException ex)
{
    Console.WriteLine($"\n [ПОМИЛКА ФАЙЛУ] Файл не знайдено: {ex.FileName}");
}

return 0;