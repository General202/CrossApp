using Core.Dto;
using Core.Import;

Console.OutputEncoding = System.Text.Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Помилка: Файл не знайдено за шляхом '{Path.GetFullPath(path)}'");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Успішно завантажено записів: {result.Items.Count}");
Console.WriteLine(new string('-', 60));

foreach (ProductDto p in result.Items.Take(5))
{
    Console.WriteLine($" {p.Id,-7} {p.Name,-25} {p.Price,10:F2} грн  {(p.Category ?? "-"),-12}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine(new string('-', 60));
    Console.WriteLine($"Пропущено пошкоджених рядків: {result.Errors.Count}");
    foreach (string err in result.Errors)
    {
        Console.WriteLine($"  ! {err}");
    }
}

return 0;