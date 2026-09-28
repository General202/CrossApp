using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        try
        {
            string json = File.ReadAllText(path);
            List<ProductDto>? items = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            var validItems = new List<ProductDto>();
            var errors = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                int index = i + 1;

                if (string.IsNullOrWhiteSpace(item.Id) || string.IsNullOrWhiteSpace(item.Name))
                {
                    errors.Add($"об'єкт {index}: ID або назва порожні");
                }
                else if (item.Price < 0)
                {
                    errors.Add($"об'єкт {index}: ціна '{item.Price}' від'ємна");
                }
                else
                {
                    validItems.Add(item);
                }
            }

            return new ImportResult<ProductDto>(validItems, errors);
        }
        catch (Exception ex)
        {
            return new ImportResult<ProductDto>([], [$"Помилка читання JSON файлу: {ex.Message}"]);
        }
    }
}