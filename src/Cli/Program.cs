using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "data", "orders.json");

// -------------------------------------------------------------------
// 1. Налаштування DI та Сервісу (з підтримкою Sinks — Додаткове завдання 1)
// -------------------------------------------------------------------
IOrderStore store = StoreFactory.CreateStore(args, jsonPath);

// Передаємо Sink у сервіс через конструктор (Manual DI)
var sinks = new List<INotificationSink>
{
    new ConsoleSink(),
    new FileSink(logPath)
};

var service = new OrderService(store, sinks);

Console.WriteLine("==========================================================");
Console.WriteLine("    ЛАБОРАТОРНА РОБОТА 6: ДОМЕННІ ПОДІЇ ТА ДЕЛЕГАТИ       ");
Console.WriteLine("==========================================================");

// -------------------------------------------------------------------
// 2. Оголошення обробників подій (Локальний метод та Замикання)
// -------------------------------------------------------------------

// А. Локальний метод підписника (для можливості подальшої відписки -=)
void OnOrderChanged(object? sender, OrderChangedEventArgs e)
{
    string logLine = $"{e.At:HH:mm:ss} | {e.Kind,-9} | {e.OrderId} | Позицій: {e.LinesCount} | Сума: {e.TotalAmount,8:F2} грн";
    
    // Форматований вивід у консоль
    Console.WriteLine($" [ПОДІЯ EVENT] {logLine}");

    // Запис у журнал logs/app.log
    string? dir = Path.GetDirectoryName(logPath);
    if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
    File.AppendAllText(logPath, logLine + Environment.NewLine);
}

// Б. Другий підписник: Лямбда-вираз із замиканням над локальною змінною
int changesCounter = 0;
EventHandler<OrderChangedEventArgs> countHandler = (_, _) => changesCounter++;

// Підписка обох обробників
service.OrderChanged += OnOrderChanged;
service.OrderChanged += countHandler;

// -------------------------------------------------------------------
// 3. Активний сценарій з подіями
// -------------------------------------------------------------------
Console.WriteLine("\n--- 1. Виконання бізнес-операцій (Підписка активна) ---");

var order1 = service.CreateOrder("CUST-101");
service.AddProductToOrder(order1.Id, "P-01", "Монітор 27\"", 9500m, 1);
service.AddProductToOrder(order1.Id, "P-02", "Кабель DisplayPort", 450m, 2);
service.ConfirmOrder(order1.Id);

Console.WriteLine($"\nУсього подій зафіксовано лічильником (Замикання): {changesCounter}");

// -------------------------------------------------------------------
// 4. Відписка від подій (Сценарій «Тихої» операції)
// -------------------------------------------------------------------
Console.WriteLine("\n--- 2. Відписка від подій (service.OrderChanged -= OnOrderChanged) ---");
service.OrderChanged -= OnOrderChanged;

var order2 = service.CreateOrder("CUST-202");
service.AddProductToOrder(order2.Id, "P-03", "Миша Logitech", 1200m, 1);

Console.WriteLine($"Подій зафіксовано після тихих операцій: {changesCounter} (збільшилось за рахунок другого підписника)");

// -------------------------------------------------------------------
// 5. Сценарій відмови (При помилці інваріанту ПОДІЯ НЕ ВИНИКАЄ)
// -------------------------------------------------------------------
Console.WriteLine("\n--- 3. Сценарій відмови (Інваріант ламається -> Подія НЕ піднімається) ---");
int counterBeforeError = changesCounter;

TryDo("Спроба додати товар з від'ємною ціною", () =>
{
    service.AddProductToOrder(order1.Id, "P-ERR", "Брак", -500m, 1);
});

Console.WriteLine($"Лічильник подій після помилки: {changesCounter} (значення не змінилося, бо подія не піднімалася)");

// -------------------------------------------------------------------
// 6. Використання Func та Action (Додаткове завдання 2 & Делегати)
// -------------------------------------------------------------------
Console.WriteLine("\n--- 4. Використання Func<Order, bool> для пошуку ---");
var expensiveOrders = service.FindOrders(o => o.TotalAmount > 5000m);
Console.WriteLine($"Знайдено замовлень на суму > 5000 грн: {expensiveOrders.Count}");

Console.WriteLine("\n--- 5. Використання Action<string> під час імпорту CSV (Додаткове завдання 2) ---");
string csvPath = Path.Combine("data", "sample.csv");
ProductCsvImporter.Load(csvPath, msg => Console.WriteLine($" [IMPORT ACTION] {msg}"));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" [ПОМИЛКА ТЕСТУ] {title}: Виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [ПЕРЕХОПЛЕНО] {title}:");
        Console.WriteLine($"    Тип: {ex.GetType().Name} | Повідомлення: {ex.Message}");
    }
}