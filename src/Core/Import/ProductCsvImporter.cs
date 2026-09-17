using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск заголовка CSV
            if (lineNumber == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {lineNumber}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 3 } => 
                new ParseFailed($"очікую щонайменше 3 колонки, отримав {parts.Length}"),

            ["", ..] or ["", ..] => 
                new ParseFailed("ID або назва товару порожні"),

            [var id, var name, var priceStr, ..] when !decimal.TryParse(priceStr, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal price) || price < 0 => 
                new ParseFailed($"ціна '{priceStr}' є некоректним або від'ємним числом"),

            [var id, var name, var priceStr] => 
                new ParseOk(new ProductDto(id, name, decimal.Parse(priceStr, CultureInfo.InvariantCulture))),

            [var id, var name, var priceStr, var category, ..] => 
                new ParseOk(new ProductDto(id, name, decimal.Parse(priceStr, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(category) ? null : category)),

            _ => new ParseFailed($"невідома помилка формату рядка")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}