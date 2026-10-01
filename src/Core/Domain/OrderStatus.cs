namespace Core.Domain;

public enum OrderStatus
{
    Draft,      // Чернетка (можна редагувати)
    Confirmed,  // Підтверджено (зафіксовано)
    Cancelled   // Скасовано
}