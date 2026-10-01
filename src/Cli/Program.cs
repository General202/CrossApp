using Core.Domain;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// -------------------------------------------------------------
// СЦЕНАРІЙ 1: УСПІШНЕ СТВОРЕННЯ ТА ЗМІНА СТАНУ
// -------------------------------------------------------------
Console.WriteLine("--- Сценарій 1: Успішне виконання доменних операцій ---");

Order order = Order.Create("ORD-2026-001", "CUST-777");
Console.WriteLine($"Створено: {order}");

// Завантаження реальних DTO з Lab 03 та перетворення
string csvPath = Path.Combine("data", "sample.csv");
if (File.Exists(csvPath))
{
    var csvResult = ProductCsvImporter.Load(csvPath);
    if (csvResult.Items.Count > 0)
    {
        order.AddProduct(csvResult.Items[0], 2);
        order.AddProduct(csvResult.Items[1], 1);
    }
}
else
{
    order.AddLine("P-001", "Ноутбук Lenovo", 35000m, 1);
    order.AddLine("P-002", "Миша бездротова", 750m, 2);
}

Console.WriteLine($"Після додавання товарів: {order}");
foreach (var line in order.Lines)
{
    Console.WriteLine($"   * {line}");
}

// Підтвердження замовлення
order.Confirm();
Console.WriteLine($"Після підтвердження: {order}");
Console.WriteLine();

// -------------------------------------------------------------
// СЦЕНАРІЙ 2: ПОРУШЕННЯ ІНВАРІАНТІВ (TRY / CATCH)
// -------------------------------------------------------------
Console.WriteLine("--- Сценарій 2: Перевірка захисту інваріантів (помилки) ---");
TryDo("1. Порожній номер замовлення", () => Order.Create("", "CUST-100"));
TryDo("2. Від'ємна ціна в рядку замовлення", () => order.AddLine("P-ERR", "Бракований товар", -100m, 1));
TryDo("3. Додавання товару у вже підтверджене замовлення", () => order.AddLine("P-003", "Клавіатура", 1500m, 1));
TryDo("4. Спроба підтвердити порожнє замовлення", () =>
{
    var emptyOrder = Order.Create("ORD-EMPTY", "CUST-100");
    emptyOrder.Confirm();
});

TryDo("5. Недопустимий перехід стану (з Cancelled у Confirmed)", () =>
{
    var cancelOrder = Order.Create("ORD-CANCEL", "CUST-100");
    cancelOrder.Cancel();
    cancelOrder.ChangeStatus(OrderStatus.Confirmed);
});

TryDo("6. Перевірка правила 2-х сутностей (ліміт замовлень клієнта)", () =>
{
    DomainImporterExtensions.ValidateMaxActiveOrdersForCustomer("CUST-777", currentActiveOrdersCount: 5, maxAllowed: 5);
});

Console.WriteLine();
Console.WriteLine("==========================================================");
Console.WriteLine($"Фінальний стан першого замовлення (не змінився): {order}");
Console.WriteLine("==========================================================");

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($" [ПОМИЛКА ТЕСТУ] {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [ПЕРЕХОПЛЕНО] {title}:");
        Console.WriteLine($"    Тип: {ex.GetType().Name} | Повідомлення: {ex.Message}");
    }
}