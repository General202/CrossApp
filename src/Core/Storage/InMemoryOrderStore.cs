using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore
{
    private readonly Dictionary<string, Order> _items = 
        (seed ?? []).ToDictionary(o => o.Id, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _items.Values.ToList();

    public Order? GetById(string id) => _items.GetValueOrDefault(id);

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з Id='{item.Id}' вже існує у сховищі.");

        _items.Add(item.Id, item);
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!_items.ContainsKey(item.Id))
            throw new InvalidOperationException($"Неможливо оновити: замовлення з Id='{item.Id}' не знайдено.");

        _items[item.Id] = item;
    }

    public bool Remove(string id) => _items.Remove(id);

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _items.Values.Where(predicate).ToList();
    }
}