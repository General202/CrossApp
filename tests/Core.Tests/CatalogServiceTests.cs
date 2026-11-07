using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public class CatalogServiceTests
{
    [Fact]
    public void AddLine_WithNegativePrice_ThrowsAndKeepsState()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("CUST-1");

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.AddProductToOrder(order.Id, "P1", "Товар", -100m, 1));
        
        // Виправлено попередження xUnit2013 (Assert.Empty замість Equal(0, Count))
        Assert.Empty(store.GetById(order.Id)!.Lines);
    }

    [Fact]
    public void ConfirmOrder_RaisesOrderChangedExactlyOnce()
    {
        // Arrange
        var store = new InMemoryOrderStore();
        var service = new OrderService(store);
        var order = service.CreateOrder("CUST-1");
        service.AddProductToOrder(order.Id, "P1", "Товар", 100m, 1);

        var events = new List<OrderChangedEventArgs>();
        service.OrderChanged += (_, e) => events.Add(e);

        // Act
        service.ConfirmOrder(order.Id);

        // Assert: Виправлено попередження xUnit2031 (передано предикат у Assert.Single)
        Assert.Single(events, e => e.Kind == OrderChangeKind.Confirmed);
    }

    [Theory]
    [InlineData(100, 3)]
    [InlineData(1000, 1)]
    public void FilterByMinAmount_CountsOnlyAboveThreshold(decimal threshold, int expected)
    {
        var orders = TestData.GetTestOrders();
        var result = Reports.ReportService.FilterByMinAmount(orders, threshold);
        Assert.Equal(expected, result.Count);
    }

    [Fact]
    public void TopByAmount_IsDeterministic()
    {
        var orders = TestData.GetTestOrders();
        var top = Reports.ReportService.TopByAmount(orders, 2);

        Assert.Equal(2, top.Count);
        Assert.Equal("ORD-T3", top[0].OrderId);
    }

    [Fact]
    public async Task ImportOrdersAsync_WhenCancelled_ThrowsOperationCanceledException()
    {
        // Arrange: Виправлено FileNotFoundException — використовуємо реальний тимчасовий файл
        string tempPath = Path.Combine(Path.GetTempPath(), $"temp_cancel_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(tempPath, "[]");

        try
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync(); // Заздалегідь скасований токен

            var store = new InMemoryOrderStore();
            var service = new OrderService(store);
            var importer = new JsonOrderImporter();

            // Act & Assert
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                service.ImportOrdersAsync(importer, tempPath, null, cts.Token));
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task LoadAsync_WithCorruptedData_ThrowsDomainException()
    {
        // Arrange: Тимчасовий файл із пошкодженим записом (ціна < 0)
        string tempPath = Path.Combine(Path.GetTempPath(), $"corrupted_{Guid.NewGuid():N}.json");
        string jsonContent = """
        [
            {
                "Id": "ORD-ERR",
                "CustomerId": "CUST-1",
                "Status": "Draft",
                "Lines": [ { "ProductId": "P1", "ProductName": "Тест", "Price": -50.0, "Quantity": 1 } ]
            }
        ]
        """;

        try
        {
            await File.WriteAllTextAsync(tempPath, jsonContent);
            var importer = new JsonOrderImporter();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() => importer.LoadAsync(tempPath));
            Assert.Contains("ORD-ERR", ex.Message);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}