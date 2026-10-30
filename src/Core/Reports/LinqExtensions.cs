namespace Core.Reports;

public static class LinqExtensions
{
    // Додаткове завдання 2: Власний метод-розширення TopBy з детермінованим сортуванням
    public static IEnumerable<T> TopByKey<T, TKey>(
        this IEnumerable<T> source,
        Func<T, TKey> keySelector,
        int count)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(keySelector);

        return source.OrderByDescending(keySelector).Take(count);
    }
}