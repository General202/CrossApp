using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Core;

Console.OutputEncoding = System.Text.Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Contains("--json"))
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };
    Console.WriteLine(JsonSerializer.Serialize(report, jsonOptions));
}
else
{
    Console.WriteLine("CrossApp - інформація про середовище");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС                 : {report.OsDescription}");
    Console.WriteLine($"Runtime            : {report.FrameworkDescription}");
    Console.WriteLine($"Архітектура        : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (визначено)    : {report.DetectedRid}");
    Console.WriteLine($"RID (від .NET)     : {report.ReportedRid}");
    Console.WriteLine($"Каталог застосунку : {report.BaseDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine("Предметна область  : Замовлення (Customer, Product, Order, OrderLine)");
}