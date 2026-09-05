using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var sysInfo = new
{
    Application = "CrossApp",
    Student = "Франецький Данило, група ФЕІ-37с",
    Domain = "Замовлення (Customer, Product, Order, OrderLine)",
    OSDescription = RuntimeInformation.OSDescription,
    OSVersion = Environment.OSVersion.ToString(),
    Architecture = RuntimeInformation.ProcessArchitecture.ToString(),
    DotNetVersion = Environment.Version.ToString(),
    FrameworkDescription = RuntimeInformation.FrameworkDescription,
    AppBaseDirectory = AppContext.BaseDirectory,
    CurrentDirectory = Environment.CurrentDirectory
};

if (args.Contains("--json"))
{
    var jsonOptions = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };
    Console.WriteLine(JsonSerializer.Serialize(sysInfo, jsonOptions));
}
else
{
    Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
    Console.WriteLine($"Студент: {sysInfo.Student}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"ОС (OSDescription): {sysInfo.OSDescription}");
    Console.WriteLine($"ОС (Environment) : {sysInfo.OSVersion}");
    Console.WriteLine($"Архітектура процесу: {sysInfo.Architecture}");
    Console.WriteLine($"Версія .NET (CLR): {sysInfo.DotNetVersion}");
    Console.WriteLine($"Runtime : {sysInfo.FrameworkDescription}");
    Console.WriteLine($"Каталог застосунку: {sysInfo.AppBaseDirectory}");
    Console.WriteLine($"Поточний каталог : {sysInfo.CurrentDirectory}");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"Предметна область: {sysInfo.Domain}");
}