using Core.Abstractions;
using Core.Events;

namespace Core.Services;

public sealed class ConsoleSink : INotificationSink
{
    public void Notify(OrderChangedEventArgs args)
    {
        Console.WriteLine($"[ConsoleSink] {args.At:HH:mm:ss} | {args.Kind,-9} | Замовлення: {args.OrderId} | Сума: {args.TotalAmount:F2} грн");
    }
}

public sealed class FileSink(string logPath) : INotificationSink
{
    private readonly string _logPath = logPath;

    public void Notify(OrderChangedEventArgs args)
    {
        string? dir = Path.GetDirectoryName(_logPath);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string line = $"{args.At:yyyy-MM-dd HH:mm:ss} | {args.Kind,-9} | {args.OrderId} | Клієнт: {args.CustomerId} | Сума: {args.TotalAmount:F2} грн";
        File.AppendAllText(_logPath, line + Environment.NewLine);
    }
}