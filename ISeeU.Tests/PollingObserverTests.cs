using ISeeU.Infrastructure.UIAutomation.WindowsOC;

namespace ISeeU.Tests;

public class PollingObserverTests
{
    // due/period = Infinite -> внутренний таймер сам не тикает, опрос дёргаем вручную через PollOnce.
    private static PollingObserver Observer(Func<string?> read, int propertyId, Action<int, object> cb) =>
        new(read, propertyId, cb, dueMs: Timeout.Infinite, periodMs: Timeout.Infinite);

    [Fact]
    public void PollOnce_FiresCallback_OnlyWhenValueChanges()
    {
        // Источник значений вместо COM value-pattern.
        var values = new Queue<string?>(new[] { "a", "a", "b", "b", null });
        var fired = new List<(int Id, object Value)>();

        var observer = Observer(values.Dequeue, 30045, (id, v) => fired.Add((id, v)));

        observer.Start();    // читает "a" -> базовое значение
        observer.PollOnce(); // "a" == "a" -> молчок
        observer.PollOnce(); // "b" != "a" -> событие "b"
        observer.PollOnce(); // "b" == "b" -> молчок
        observer.PollOnce(); // null != "b" -> событие "" (null нормализуется)
        observer.Stop();

        Assert.Equal(2, fired.Count);
        Assert.Equal((30045, (object)"b"), fired[0]);
        Assert.Equal((30045, (object)""), fired[1]);
    }

    [Fact]
    public void Start_ReadsInitialValue_AndStopDoesNotThrow()
    {
        var reads = 0;
        var observer = Observer(() => { reads++; return "x"; }, 1, (_, _) => { });

        observer.Start();
        Assert.Equal(1, reads); // Start один раз прочитал стартовое значение

        var ex = Record.Exception(() => observer.Stop());
        Assert.Null(ex);
    }

    [Fact]
    public void PollOnce_DoesNotFire_WhenValueStaysSame()
    {
        var fired = 0;
        var observer = Observer(() => "const", 1, (_, _) => fired++);

        observer.Start();
        observer.PollOnce();
        observer.PollOnce();
        observer.Stop();

        Assert.Equal(0, fired);
    }
}
