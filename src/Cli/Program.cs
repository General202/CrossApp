using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string jsonPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

// Composition Root: Ручне Впровадження Залежностей (Manual DI)
IOrderStore store = StoreFactory.CreateStore(args, jsonPath);
var service = new OrderService(store);

Console.WriteLine("==========================================================");
Console.WriteLine($"  СХОВИЩЕ: {store.GetType().Name}");
Console.WriteLine("==========================================================");

// 1. Успішний сценарій
Console.WriteLine("\n--- 1. Створення нового замовлення через Сервіс ---");
var newOrder = service.CreateOrder("CUST-999");
service.AddProductToOrder(newOrder.Id, "P-100", "Флешка 128GB", 450m, 2);
service.ConfirmOrder(newOrder.Id);
Console.WriteLine($"Створено та підтверджено: {service.GetOrder(newOrder.Id)}");

// 2. Вивід усіх замовлень
Console.WriteLine("\n--- 2. Список усіх замовлень у сховищі ---");
foreach (var order in service.GetAllOrders())
{
    Console.WriteLine($"  [{order.Status,-9}] {order.Id} | Клієнт: {order.CustomerId,-8} | Сума: {order.TotalAmount,10:F2} грн");
}

// 3. Пошук з предикатом (Додаткове завдання 2)
Console.WriteLine("\n--- 3. Пошук підтверджених замовлень (Func<Order, bool>) ---");
var confirmedOrders = service.GetOrdersByStatus(OrderStatus.Confirmed);
Console.WriteLine($"Знайдено підтверджених: {confirmedOrders.Count}");

// 4. Сценарій помилок (Try / Catch)
Console.WriteLine("\n--- 4. Сценарій відмови (Спроба додати товар у неіснуюче замовлення) ---");
TryDo("Додавання товару у неіснуюче замовлення", () => 
    service.AddProductToOrder("ORD-NOT-FOUND", "P-01", "Товар", 100m, 1));

TryDo("Спроба додати дублікат Id у сховище", () => 
    store.Add(Order.Create(newOrder.Id, "CUST-ERR")));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [ПОМИЛКА ТЕСТУ] {title}: Виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [ПЕРЕХОПЛЕНО] {title}:");
        Console.WriteLine($"     Тип: {ex.GetType().Name} | Повідомлення: {ex.Message}");
    }
}