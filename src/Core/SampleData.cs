using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Order> GetOrders()
    {
        var orders = new List<Order>();

        // Створюємо 18 замовлень для повноцінної аналітики
        var data = new (string Id, string Cust, OrderStatus Status, (string PId, string Name, decimal Price, int Qty)[] Items)[]
        {
            ("ORD-1001", "CUST-001", OrderStatus.Confirmed, [("P-01", "Ноутбук Lenovo", 38000m, 1), ("P-02", "Миша", 800m, 2)]),
            ("ORD-1002", "CUST-002", OrderStatus.Draft, [("P-03", "Клавіатура", 3500m, 1)]),
            ("ORD-1003", "CUST-001", OrderStatus.Draft, [("P-04", "Монітор 27\"", 12500m, 2)]),
            ("ORD-1004", "CUST-003", OrderStatus.Cancelled, [("P-05", "Навушники Sony", 9000m, 1)]),
            ("ORD-1005", "CUST-004", OrderStatus.Confirmed, [("P-06", "Кабель HDMI", 300m, 3)]),
            ("ORD-1006", "CUST-002", OrderStatus.Confirmed, [("P-07", "Планшет iPad", 24000m, 1)]),
            ("ORD-1007", "CUST-005", OrderStatus.Draft, [("P-08", "Чохол", 500m, 1)]),
            ("ORD-1008", "CUST-001", OrderStatus.Confirmed, [("P-09", "Флешка 128GB", 450m, 4)]),
            ("ORD-1009", "CUST-003", OrderStatus.Confirmed, [("P-10", "Веб-камера", 2800m, 1)]),
            ("ORD-1010", "CUST-006", OrderStatus.Cancelled, [("P-11", "Блок живлення", 3200m, 1)]),
            ("ORD-1011", "CUST-004", OrderStatus.Draft, [("P-12", "Колонки", 1800m, 2)]),
            ("ORD-1012", "CUST-002", OrderStatus.Confirmed, [("P-13", "SSD 1TB", 3600m, 2)]),
            ("ORD-1013", "CUST-005", OrderStatus.Confirmed, [("P-14", "Рюкзак", 2200m, 1)]),
            ("ORD-1014", "CUST-001", OrderStatus.Cancelled, [("P-15", "Зарядне 65W", 1100m, 1)]),
            ("ORD-1015", "CUST-006", OrderStatus.Confirmed, [("P-16", "Смартфон", 19500m, 1)]),
            ("ORD-1016", "CUST-003", OrderStatus.Draft, [("P-17", "Підставка", 600m, 1)]),
            ("ORD-1017", "CUST-004", OrderStatus.Confirmed, [("P-18", "Хаб USB-C", 1400m, 2)]),
            ("ORD-1018", "CUST-005", OrderStatus.Confirmed, [("P-19", "Мікрофон", 4200m, 1)])
        };

        foreach (var d in data)
        {
            var order = Order.Create(d.Id, d.Cust);
            foreach (var item in d.Items)
            {
                order.AddLine(item.PId, item.Name, item.Price, item.Qty);
            }

            if (d.Status == OrderStatus.Confirmed) order.Confirm();
            else if (d.Status == OrderStatus.Cancelled) order.Cancel();

            orders.Add(order);
        }

        return orders;
    }
}