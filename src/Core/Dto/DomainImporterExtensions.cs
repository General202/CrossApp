using Core.Domain;
using Core.Dto;

namespace Core.Import;

public static class DomainImporterExtensions
{
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