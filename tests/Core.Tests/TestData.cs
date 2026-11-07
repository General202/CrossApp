using Core.Domain;

namespace Core.Tests;

public static class TestData
{
    public static List<Order> GetTestOrders()
    {
        var o1 = Order.Create("ORD-T1", "CUST-1");
        o1.AddLine("P1", "Товар 1", 100m, 2);
        o1.Confirm();

        var o2 = Order.Create("ORD-T2", "CUST-2");
        o2.AddLine("P2", "Товар 2", 500m, 1);

        var o3 = Order.Create("ORD-T3", "CUST-1");
        o3.AddLine("P3", "Товар 3", 1500m, 1);

        return [o1, o2, o3];
    }
}