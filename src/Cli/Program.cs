using System;
using System.Runtime.InteropServices;

// Гарантує правильне відображення кирилиці у консолі Windows[cite: 1]
Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("CrossApp - практикум з крос-платформного програмування");
Console.WriteLine("Студент: Франецький Данило, група ФЕІ-37с");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС (OSDescription): {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment) : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу: {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR): {Environment.Version}");
Console.WriteLine($"Runtime : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку: {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог : {Environment.CurrentDirectory}");
Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення (Customer, Product, Order, OrderLine)");