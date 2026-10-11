using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class OrderService(IOrderStore store)
{
    private readonly IOrderStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Order CreateOrder(string customerId)
    {
        string id = $"ORD-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        var order = Order.Create(id, customerId);
        _store.Add(order);
        return order;
    }

    public void AddProductToOrder(string orderId, string productId, string productName, decimal price, int quantity)
    {
        var order = _store.GetById(orderId) 
            ?? throw new InvalidOperationException($"Замовлення з Id='{orderId}' не знайдено.");

        order.AddLine(productId, productName, price, quantity);
        _store.Update(order);
    }

    public void ConfirmOrder(string orderId)
    {
        var order = _store.GetById(orderId) 
            ?? throw new InvalidOperationException($"Замовлення з Id='{orderId}' не знайдено.");

        order.Confirm();
        _store.Update(order);
    }

    public IReadOnlyList<Order> GetAllOrders() => _store.List();

    public Order? GetOrder(string id) => _store.GetById(id);

    // Використання Func<Order, bool> для гнучкого пошуку
    public IReadOnlyList<Order> GetOrdersByStatus(OrderStatus status) => 
        _store.Find(o => o.Status == status);

    public IReadOnlyList<Order> GetOrdersByCustomer(string customerId) => 
        _store.Find(o => o.CustomerId.Equals(customerId, StringComparison.OrdinalIgnoreCase));
}