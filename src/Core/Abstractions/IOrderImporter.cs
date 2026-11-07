using Core.Domain;

namespace Core.Abstractions;

public interface IOrderImporter
{
    Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default);
    
    // Додаткове завдання 1: Потоковий імпорт через IAsyncEnumerable
    IAsyncEnumerable<Order> LoadStreamAsync(string path, CancellationToken ct = default);
}