namespace Core.Events;

public enum OrderChangeKind
{
    Created,     // Замовлення створено
    LineAdded,   // Додано позицію
    Confirmed,   // Підтверджено
    Cancelled    // Скасовано
}