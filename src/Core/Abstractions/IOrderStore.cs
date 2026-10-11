using Core.Domain;

namespace Core.Abstractions;

public interface IOrderStore
{
    IReadOnlyList<Order> List();
    Order? GetById(string id);
    void Add(Order item);
    void Update(Order item);
    bool Remove(string id);

    // Додаткове завдання 2: Пошук з параметром-предикатом Func<Order, bool>
    IReadOnlyList<Order> Find(Func<Order, bool> predicate);
}