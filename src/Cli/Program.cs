using System.Reflection;
using Core;
using Core.Abstractions;
using Core.Configuration;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// -------------------------------------------------------------------
// 1. Побудова стеку конфігурацій (appsettings -> env -> secrets -> CLI)
// -------------------------------------------------------------------
string envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{envName}.json", optional: true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables(prefix: "CROSSAPP_")
    .AddCommandLine(args)
    .Build();

var options = config.Get<AppOptions>() ?? new AppOptions();

// Перевірка спрощеної змінної CROSSAPP_DATA_PATH
string? legacyDataPath = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
var storageOpts = new StorageOptions
{
    DataPath = !string.IsNullOrWhiteSpace(legacyDataPath) ? legacyDataPath : options.Storage.DataPath,
    FileName = options.Storage.FileName
};

string fullDataPath = storageOpts.ResolveFullPath(AppContext.BaseDirectory);
string? apiKey = config["Integration:ApiKey"];

Console.WriteLine("==========================================================");
Console.WriteLine($"   СЕРЕДОВИЩЕ: {options.EnvironmentName} | ВАЛЮТА: {options.Orders.Currency}");
Console.WriteLine($"   ФАЙЛ ДАНИХ: {fullDataPath}");
Console.WriteLine(apiKey is null 
    ? "   ApiKey: НЕ заданий" 
    : $"   ApiKey: ПРИСУТНІЙ, довжина {apiKey.Length} символів");
Console.WriteLine("==========================================================");

// -------------------------------------------------------------------
// 2. Ініціалізація сховища та сервісів
// -------------------------------------------------------------------
IOrderStore store = new FileOrderStore(fullDataPath);
var service = new OrderService(store);

var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "export";

if (command == "export")
{
    // Заповнюємо початковими даними, якщо сховище порожнє
    if (service.GetAllOrders().Count == 0)
    {
        foreach (var sampleOrder in SampleData.GetOrders())
        {
            store.Add(sampleOrder);
        }
    }

    var list = service.GetAllOrders();
    Console.WriteLine($"\nУспішно експортовано замовлень: {list.Count}");
}
else if (command == "report-top")
{
    var lines = ReportService.TopByAmount(service.GetAllOrders(), 5);
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.OrderId,-14} | {l.CustomerId,-10} | {l.Status,-10} | {l.TotalAmount,12:F2} грн");
    }
}

return 0;