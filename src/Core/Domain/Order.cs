using Core.Dto;

namespace Core.Domain;

public sealed class Order
{
    private readonly List<OrderLine> _lines = [];

    public string Id { get; }
    public string CustomerId { get; }
    public OrderStatus Status { get; private set; }

    // Захищена колекція: назовні повертається IReadOnlyList, прихований внутрішній List
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public decimal TotalAmount => _lines.Sum(l => l.TotalPrice);

    private Order(string id, string customerId, OrderStatus status)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
    }

    // Фабричний метод створення замовлення
    public static Order Create(string id, string customerId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер замовлення є обов'язковим", nameof(id));

        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentException("Ідентифікатор клієнта є обов'язковим", nameof(customerId));

        return new Order(id.Trim().ToUpperInvariant(), customerId.Trim(), OrderStatus.Draft);
    }

    // Додавання рядка до замовлення (з перевіркою інваріантів)
    public void AddLine(string productId, string productName, decimal price, int quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException($"Неможливо додати товар: замовлення {Id} перебуває у стані '{Status}' і недоступне для редагування");

        var line = OrderLine.Create(productId, productName, price, quantity);
        _lines.Add(line);
    }

    // Додавання вже готового DTO товару (інтеграція з тижнем 3)
    public void AddProduct(ProductDto product, int quantity)
    {
        ArgumentNullException.ThrowIfNull(product);
        AddLine(product.Id, product.Name, product.Price, quantity);
    }

    // Контроль переходів станів через switch expression (Додаткове завдання 3)
    public void ChangeStatus(OrderStatus newStatus)
    {
        bool isValidTransition = (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Confirmed) when _lines.Count > 0 => true,
            (OrderStatus.Draft, OrderStatus.Confirmed) when _lines.Count == 0 => 
                throw new InvalidOperationException("Неможливо підтвердити порожнє замовлення без товарів"),
            (OrderStatus.Draft, OrderStatus.Cancelled) => true,
            (OrderStatus.Confirmed, OrderStatus.Cancelled) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException($"Недопустимий перехід стану замовлення з '{Status}' у '{newStatus}'");

        Status = newStatus;
    }

    public void Confirm() => ChangeStatus(OrderStatus.Confirmed);
    public void Cancel() => ChangeStatus(OrderStatus.Cancelled);

    // Мапінг у DTO тижня 3 (для підтримки сховищ)
    public OrderDto ToDto() => new(Id, CustomerId, TotalAmount, Status.ToString(), _lines.Count);

    // Відновлення сутності з DTO через перевірки інваріантів
    public static Order FromDto(OrderDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        var order = Create(dto.Id, dto.CustomerId);
        if (Enum.TryParse<OrderStatus>(dto.Status, out var parsedStatus) && parsedStatus != OrderStatus.Draft)
        {
            // Для тестування відновлюємо статус без збою, якщо замовлення вже зафіксоване
            order.Status = parsedStatus;
        }
        return order;
    }

    public override string ToString() => $"Замовлення {Id} | Клієнт: {CustomerId} | Статус: {Status} | Позицій: {_lines.Count} | Сума: {TotalAmount:F2} грн";
}

// Допоміжний DTO під замовлення
public record OrderDto(string Id, string CustomerId, decimal TotalAmount, string Status, int LinesCount);