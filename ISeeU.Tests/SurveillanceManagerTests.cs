using ConnectInfo;
using ISeeU.Application.Services;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Tests;

public class SurveillanceManagerTests
{
    private static FakeElement Element(int pid, string name, int x = 0, int y = 0) => new()
    {
        ProcessId = pid,
        Name = name,
        BoundingRectangle = new Rect { X = x, Y = y, Width = 10, Height = 10 }
    };

    [Fact]
    public void Add_StartsObserver_AndStoresRule()
    {
        var element = Element(1, "A");
        var provider = new FakeProvider { FindElementResult = element };
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);

        manager.Add(element, 30005, (_, _) => { });

        Assert.Single(fabric.Observers);
        Assert.Equal(1, fabric.Observers[0].StartCount);
        Assert.Equal(30005, fabric.Created[0].PropertyId);
        Assert.Same(element, fabric.Created[0].Element);
    }

    [Fact]
    public void UnSubscribe_StopsAndRemoves_OnlyMatchingRule()
    {
        var a = Element(1, "A", 0, 0);
        var b = Element(2, "B", 100, 100);
        var provider = new FakeProvider();
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);

        manager.Add(a, 30005, (_, _) => { });
        manager.Add(b, 30005, (_, _) => { });

        // UnSubscribe ищет элемент по координатам через провайдер.
        provider.FindElementResult = a;
        manager.UnSubscribe(new ElementInfo { X = 0, Y = 0 });

        Assert.Equal(1, fabric.Observers[0].StopCount); // правило A погашено
        Assert.Equal(0, fabric.Observers[1].StopCount); // правило B осталось
    }

    [Fact]
    public void UnSubscribe_WhenNothingMatches_RemovesNothing()
    {
        var a = Element(1, "A");
        var other = Element(99, "Other", 500, 500);
        var provider = new FakeProvider { FindElementResult = other };
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);

        manager.Add(a, 30005, (_, _) => { });
        manager.UnSubscribe(new ElementInfo { X = 500, Y = 500 });

        Assert.Equal(0, fabric.Observers[0].StopCount);
    }

    [Fact]
    public async Task CheckAllElementIsAlive_RemovesDeadRules()
    {
        var alive = Element(1, "Alive");
        var dead = Element(2, "Dead");
        var provider = new FakeProvider
        {
            IsAliveFunc = e => ReferenceEquals(e, alive)
        };
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);

        manager.Add(alive, 30005, (_, _) => { });
        manager.Add(dead, 30005, (_, _) => { });

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(1200); // даём отработать одной итерации (Task.Delay(1000))
        try
        {
            await manager.CheckAllElementIsAlive(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // ожидаемо: цикл прерывается на следующем Task.Delay
        }

        Assert.Equal(0, fabric.Observers[0].StopCount); // живой остался
        Assert.Equal(1, fabric.Observers[1].StopCount); // мёртвый погашен
    }
}
