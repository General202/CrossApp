using System.Runtime.CompilerServices;
using System.Text.Json;
using Core.Abstractions;
using Core.Domain;
using Core.Storage;

namespace Core.Storage;

public sealed class JsonOrderImporter : IOrderImporter
{
    public async Task<IReadOnlyList<Order>> LoadAsync(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл '{path}' не знайдено.", path);

        // Справжнє асинхронне читання файлу без Task.Run
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

        var dtos = await JsonSerializer.DeserializeAsync<List<OrderStorageDto>>(stream, cancellationToken: ct)
            ?? throw new DomainException($"Файл {Path.GetFileName(path)} порожній або містить некоректний JSON.");

        var result = new List<Order>(dtos.Count);

        for (int i = 0; i < dtos.Count; i++)
        {
            // Перевірка сигналу скасування
            ct.ThrowIfCancellationRequested();

            try
            {
                result.Add(OrderStorageDto.ToDomain(dtos[i]));
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                throw new DomainException($"Запис {i + 1} (Id='{dtos[i].Id}'): {ex.Message}", ex);
            }
        }

        return result;
    }

    // Додаткове завдання 1: Реалізація IAsyncEnumerable із [EnumeratorCancellation]
    public async IAsyncEnumerable<Order> LoadStreamAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var orders = await LoadAsync(path, ct);
        foreach (var order in orders)
        {
            ct.ThrowIfCancellationRequested();
            yield return order;
            await Task.Delay(10, ct); // Емуляція асинхронного потокового прочитання
        }
    }
}