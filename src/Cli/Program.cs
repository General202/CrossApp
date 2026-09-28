using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Помилка: Файл не знайдено за шляхом '{Path.GetFullPath(path)}'");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// Вибір імпортера за розширенням файлу через switch expression
ImportResult<ProductDto> result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => new ImportResult<ProductDto>([], [$"Непідтримуваний формат файлу: {extension}"])
};

// Статистика імпорту
int accepted = result.Items.Count;
int skipped = result.Errors.Count;
int total = accepted + skipped;
double errorRate = total > 0 ? (double)skipped / total * 100 : 0;

Console.WriteLine($"Формат: {extension.ToUpper()} | Статистика: Усього: {total} | Прийнято: {accepted} | Пропущено: {skipped} | Помилок: {errorRate:F1}%");
Console.WriteLine(new string('-', 70));

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-7} {p.Name,-28} {p.Price,10:F2} грн  {(p.Category ?? "-"),-12}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(new string('-', 70));
    Console.WriteLine($"Пропущені рядки / об'єкти ({result.Errors.Count}):");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
}

return 0;