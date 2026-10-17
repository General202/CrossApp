using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class ProductCsvImporter
{
    private const char Separator = ';';

    // Додано опціональний параметр Action<string>? progressReport
public static ImportResult<ProductDto> Load(string path, Action<string>? progressReport = null)
{
    var items = new List<ProductDto>();
    var errors = new List<string>();

    if (!File.Exists(path))
    {
        progressReport?.Invoke($"[IMPORTER ERROR] Файл не знайдено: {path}");
        return new ImportResult<ProductDto>(items, [$"Файл '{path}' не знайдено."]);
    }

    string[] lines = File.ReadAllLines(path);

    for (int i = 0; i < lines.Length; i++)
    {
        int lineNumber = i + 1;
        string line = lines[i];

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
            continue;

        if (lineNumber == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
            continue;

        switch (ParseLine(line))
        {
            case ParseOk ok:
                items.Add(ok.Value);
                break;
            case ParseFailed failed:
                string errMessage = $"рядок {lineNumber}: {failed.Reason}";
                errors.Add(errMessage);
                progressReport?.Invoke($"[CSV SKIP] {errMessage}"); // сповіщення через Action
                break;
        }
    }

    progressReport?.Invoke($"[IMPORTER DONE] Завантажено {items.Count} записів, пропущено {errors.Count}.");
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