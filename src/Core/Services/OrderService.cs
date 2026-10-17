using Core.Abstractions;
using Core.Domain;
using Core.Events;

namespace Core.Services;

public sealed class OrderService(IOrderStore store, IEnumerable<INotificationSink>? sinks = null)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));
    private readonly List<INotificationSink> _sinks = sinks?.ToList() ?? [];

    // Доменна подія змінення стану замовлення
    public event EventHandler<OrderChangedEventArgs>? OrderChanged;

    private void OnOrderChanged(Order order, OrderChangeKind kind)
    {
        var args = new OrderChangedEventArgs(order.Id, kind, order.CustomerId, order.TotalAmount, order.Lines.Count);
        
        // 1. Підняття стандартної .NET події через умовний виклик ?.Invoke
        OrderChanged?.Invoke(this, args);

        // 2. Сповіщення підключених через DI приймачів (Додаткове завдання 1)
        foreach (var sink in _sinks)
        {
            try
            {
                sink.Notify(args);
            }
            catch
            {
                // Помилка в приймачеві не повинна ламати бізнес-операцію
            }
        }
    }

    public Order CreateOrder(string customerId)
    {
        string id = $"ORD-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var order = Order.Create(id, customerId);
        _store.Add(order);

        // Подія піднімається виключно після успішної зміни
        OnOrderChanged(order, OrderChangeKind.Created);
        return order;
    }

    public void AddProductToOrder(string orderId, string productId, string productName, decimal price, int quantity)
    {
        var order = _store.GetById(orderId) 
            ?? throw new InvalidOperationException($"Замовлення з Id='{orderId}' не знайдено.");

        order.AddLine(productId, productName, price, quantity); // якщо інваріант кине виняток — події НЕ буде
        _store.Update(order);

        OnOrderChanged(order, OrderChangeKind.LineAdded);
    }

    public void ConfirmOrder(string orderId)
    {
        var order = _store.GetById(orderId) 
            ?? throw new InvalidOperationException($"Замовлення з Id='{orderId}' не знайдено.");

        order.Confirm();
        _store.Update(order);

        OnOrderChanged(order, OrderChangeKind.Confirmed);
    }

    public void CancelOrder(string orderId)
    {
        var order = _store.GetById(orderId) 
            ?? throw new InvalidOperationException($"Замовлення з Id='{orderId}' не знайдено.");

        order.Cancel();
        _store.Update(order);

        OnOrderChanged(order, OrderChangeKind.Cancelled);
    }

    public IReadOnlyList<Order> GetAllOrders() => _store.List();

    public Order? GetOrder(string id) => _store.GetById(id);

    // Використання Func<Order, bool> делегату для розширеного пошуку
    public IReadOnlyList<Order> FindOrders(Func<Order, bool> predicate) => 
        _store.Find(predicate);
}