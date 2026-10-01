using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImporterExtensions
{
    // Додаткове завдання 1: Перетворення ImportResult<ProductDto> у перевірені сутності + помилки інваріантів
    public static (List<OrderLine> ValidLines, List<string> DomainErrors) ConvertToOrderLines(
        this ImportResult<ProductDto> importResult, int defaultQuantity = 1)
    {
        var validLines = new List<OrderLine>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                var line = OrderLine.Create(dto.Id, dto.Name, dto.Price, defaultQuantity);
                validLines.Add(line);
            }
            catch (Exception ex)
            {
                errors.Add($"Товар '{dto.Id}': Доменна помилка — {ex.Message}");
            }
        }

        return (validLines, errors);
    }

    // Додаткове завдання 2: Перевірка правила, що охоплює ДВІ сутності (Клієнт та Замовлення)
    // Пояснення для звіту: Такі перевірки виносяться у сервіс, бо сутність Замовлення не повинна
    // мати прямих посилань на базу даних всіх замовлень клієнта для перевірки їх кількості.
    public static bool ValidateMaxActiveOrdersForCustomer(string customerId, int currentActiveOrdersCount, int maxAllowed = 5)
    {
        if (currentActiveOrdersCount >= maxAllowed)
        {
            throw new InvalidOperationException(
                $"Клієнт '{customerId}' досяг ліміту відкритих замовлень ({maxAllowed}). Створення нового замовлення відхилено.");
        }
        return true;
    }
}