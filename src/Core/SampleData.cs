using Core.Domain;

namespace Core;

public static class SampleData
{
    public static List<Order> GetOrders()
    {
        var o1 = Order.Create("ORD-1001", "CUST-001");
        o1.AddLine("P-01", "Ноутбук Lenovo", 38000m, 1);
        o1.AddLine("P-02", "Бездротова миша", 8000m, 2);
        o1.Confirm();

        var o2 = Order.Create("ORD-1002", "CUST-002");
        o2.AddLine("P-03", "Клавіатура Keychron", 3500m, 1);

        var o3 = Order.Create("ORD-1003", "CUST-001");
        o3.AddLine("P-04", "Монітор Dell 27", 12500m, 2);

        var o4 = Order.Create("ORD-1004", "CUST-003");
        o4.AddLine("P-05", "Навушники Sony", 9000m, 1);
        o4.Cancel();

        var o5 = Order.Create("ORD-1005", "CUST-004");
        o5.AddLine("P-06", "Кабель HDMI", 300m, 3);
        o5.Confirm();

        return [o1, o2, o3, o4, o5];
    }
}