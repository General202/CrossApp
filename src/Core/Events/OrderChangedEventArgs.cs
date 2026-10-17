using Core.Events;

namespace Core.Events;

public sealed class OrderChangedEventArgs(
    string orderId,
    OrderChangeKind kind,
    string customerId,
    decimal totalAmount,
    int linesCount) : EventArgs
{
    public string OrderId { get; } = orderId;
    public OrderChangeKind Kind { get; } = kind;
    public string CustomerId { get; } = customerId;
    public decimal TotalAmount { get; } = totalAmount;
    public int LinesCount { get; } = linesCount;
    public DateTimeOffset At { get; } = DateTimeOffset.Now;
}