using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

// Декоратор заучує запити GetById, зменшуючи звернення до внутрішнього сховища
public sealed class CachingOrderStore(IOrderStore innerStore) : IOrderStore
{
    private readonly IOrderStore _inner = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<Order> List() => _inner.List();

    public Order? GetById(string id)
    {
        if (_cache.TryGetValue(id, out var cached))
            return cached;

        var order = _inner.GetById(id);
        if (order != null)
        {
            _cache[id] = order;
        }
        return order;
    }

    public void Add(Order item)
    {
        _inner.Add(item);
        _cache[item.Id] = item;
    }

    public void Update(Order item)
    {
        _inner.Update(item);
        _cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        _cache.Remove(id);
        return _inner.Remove(id);
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate) => _inner.Find(predicate);
}