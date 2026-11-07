using Xunit;

namespace Core.Tests;

public class LinkedTokenTests
{
    [Fact]
    public async Task LinkedTokenSource_TriggersCancellationOnTimeout()
    {
        // Arrange
        using var userCts = new CancellationTokenSource();
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

        // Act & Assert
        await Task.Delay(100);
        Assert.True(linkedCts.IsCancellationRequested);
        Assert.True(timeoutCts.IsCancellationRequested);
        Assert.False(userCts.IsCancellationRequested); // Перевірка, що спрацював саме таймаут
    }
}