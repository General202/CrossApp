using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;

public sealed class FileOrderStore(string path) : IOrderStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };

    private readonly Dictionary<string, Order> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _path = Path.GetFullPath(path);
    private bool _loaded;

    private void EnsureLoaded()
    {
        if (_loaded) return;

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            var dtos = JsonSerializer.Deserialize<List<OrderStorageDto>>(json, Options) ?? [];

            _cache.Clear();
            foreach (var dto in dtos)
            {
                var order = OrderStorageDto.ToDomain(dto);
                _cache[order.Id] = order;
            }
        }

        _loaded = true;
    }

    private void Flush()
    {
        string? directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var dtos = _cache.Values.Select(OrderStorageDto.FromDomain).ToList();
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));
    }

    public IReadOnlyList<Order> List()
    {
        EnsureLoaded();
        return _cache.Values.ToList();
    }

    public Order? GetById(string id)
    {
        EnsureLoaded();
        return _cache.GetValueOrDefault(id);
    }

    public void Add(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        if (_cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Замовлення з Id='{item.Id}' вже існує у файловому сховищі.");

        _cache.Add(item.Id, item);
        Flush();
    }

    public void Update(Order item)
    {
        ArgumentNullException.ThrowIfNull(item);
        EnsureLoaded();

        _cache[item.Id] = item;
        Flush();
    }

    public bool Remove(string id)
    {
        EnsureLoaded();
        if (!_cache.Remove(id)) return false;

        Flush();
        return true;
    }

    public IReadOnlyList<Order> Find(Func<Order, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        EnsureLoaded();
        return _cache.Values.Where(predicate).ToList();
    }
}

// Допоміжний DTO під повне збереження Агрегату з рядками
public record OrderStorageDto(
    string Id,
    string CustomerId,
    string Status,
    List<OrderLineStorageDto> Lines)
{
    public static OrderStorageDto FromDomain(Order order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.Lines.Select(l => new OrderLineStorageDto(l.ProductId, l.ProductName, l.Price, l.Quantity)).ToList()
    );

    public static Order ToDomain(OrderStorageDto dto)
    {
        var order = Order.Create(dto.Id, dto.CustomerId);
        foreach (var l in dto.Lines)
        {
            order.AddLine(l.ProductId, l.ProductName, l.Price, l.Quantity);
        }

        if (Enum.TryParse<OrderStatus>(dto.Status, out var status) && status != OrderStatus.Draft)
        {
            if (status == OrderStatus.Confirmed) order.Confirm();
            else if (status == OrderStatus.Cancelled) order.Cancel();
        }

        return order;
    }
}

public record OrderLineStorageDto(string ProductId, string ProductName, decimal Price, int Quantity);