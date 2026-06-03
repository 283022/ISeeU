using ConnectInfo;
using ISeeU.Application.Convertor;
using ISeeU.Application.Services;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Tests;

public class SurveillanceServiceTests
{
    private static (SurveillanceService service, FakeProvider provider, FakeTargetFabric fabric)
        Build(FakeElement element)
    {
        var provider = new FakeProvider { FindElementResult = element };
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);
        var convertor = new Convertor(provider);
        var service = new SurveillanceService(manager, convertor, provider);
        return (service, provider, fabric);
    }

    [Fact]
    public async Task FindElementAsync_ReturnsInfo_WithClickedCoordinates()
    {
        var element = new FakeElement
        {
            Name = "Button",
            BoundingRectangle = new Rect { X = 999, Y = 999, Width = 40, Height = 20 }
        };
        var (service, _, _) = Build(element);

        var info = await service.FindElementAsync(7, 11);

        Assert.Equal("Button", info.Name);
        Assert.Equal(7, info.X);   // координаты клика, а не угол bounding-rect
        Assert.Equal(11, info.Y);
        Assert.Equal(40, info.Width);
        Assert.Equal(20, info.Height);
        Assert.False(string.IsNullOrWhiteSpace(info.ElementId));
        Assert.True(Guid.TryParse(info.ElementId, out _));
    }

    [Fact]
    public async Task GetSupportedPropertiesAsync_CombinesPotentialAndPatternProperties()
    {
        var element = new FakeElement();
        var (service, provider, _) = Build(element);

        provider.SupportedProperties = new Dictionary<int, string>
        {
            { 30005, "Name" },
            { 30010, "IsEnabled" }
        };
        provider.CurrentValueFunc = (_, id) => id switch
        {
            30043 => true,  // IsValuePatternAvailable -> Value поддерживается
            30041 => false, // IsTogglePatternAvailable -> ToggleState нет
            _ => false
        };

        var ids = await service.GetSupportedPropertiesAsync(new ElementInfo { X = 1, Y = 1 });

        Assert.Contains(30005, ids);
        Assert.Contains(30010, ids);
        Assert.Contains(30045, ids);       // Value через паттерн
        Assert.DoesNotContain(30022, ids); // не в potential-set
        Assert.DoesNotContain(30086, ids); // ToggleState недоступен
    }

    [Fact]
    public async Task GetSupportedPropertiesAsync_TreatsNonBoolAndNullAvailability()
    {
        var element = new FakeElement();
        var (service, provider, _) = Build(element);

        provider.SupportedProperties = new Dictionary<int, string>();
        provider.CurrentValueFunc = (_, id) => id switch
        {
            30041 => (object)1,  // non-bool truthy -> Convert.ToBoolean -> true
            30043 => null!,      // null -> false
            _ => null!
        };

        var ids = await service.GetSupportedPropertiesAsync(new ElementInfo { X = 1, Y = 1 });

        Assert.Contains(30086, ids);       // ToggleState (avail == 1)
        Assert.DoesNotContain(30045, ids); // Value (avail == null)
    }

    [Fact]
    public async Task SubscribeAsync_AddsRulesAndPushesChangesToClient()
    {
        var element = new FakeElement { Name = "Win" };
        var (service, _, fabric) = Build(element);
        var client = new FakeClient();
        service.Client = client;

        var info = new ElementInfo
        {
            Name = "Win",
            X = 1,
            Y = 1,
            Properties = new List<PropertyInfo>
            {
                new() { Id = 30005, Name = "Name" },
                new() { Id = 30045, Name = "Value" }
            }
        };

        await service.SubscribeAsync(info);

        Assert.Equal(2, fabric.Observers.Count);
        Assert.Equal(1, fabric.Observers[0].StartCount);

        // Симулируем срабатывание наблюдателя -> push клиенту.
        fabric.Created[0].Callback(30005, "hello");
        Assert.Contains(("Win", "Name", "hello"), client.Calls);

        // null-значение должно превратиться в пустую строку.
        fabric.Created[1].Callback(30045, null!);
        Assert.Contains(("Win", "Value", ""), client.Calls);
    }

    [Fact]
    public async Task SubscribeAsync_WithNullClient_DoesNotThrowOnCallback()
    {
        var element = new FakeElement { Name = "Win" };
        var (service, _, fabric) = Build(element);

        var info = new ElementInfo
        {
            Name = "Win",
            Properties = new List<PropertyInfo> { new() { Id = 30005, Name = "Name" } }
        };

        await service.SubscribeAsync(info);

        var ex = Record.Exception(() => fabric.Created[0].Callback(30005, "x"));
        Assert.Null(ex);
    }

    [Fact]
    public async Task SubscribeAsync_WithNullProperties_DoesNothing()
    {
        var element = new FakeElement();
        var (service, _, fabric) = Build(element);

        await service.SubscribeAsync(new ElementInfo { Properties = null! });

        Assert.Empty(fabric.Observers);
    }

    [Fact]
    public async Task SubscribeAsync_WithNullInfo_DoesNothing()
    {
        var element = new FakeElement();
        var (service, _, fabric) = Build(element);

        await service.SubscribeAsync(null!);

        Assert.Empty(fabric.Observers);
    }

    [Fact]
    public async Task UnsubscribeAsync_DelegatesToManager()
    {
        var element = new FakeElement { Name = "Win", BoundingRectangle = new Rect { X = 1, Y = 1 } };
        var (service, _, fabric) = Build(element);

        await service.SubscribeAsync(new ElementInfo
        {
            Name = "Win",
            X = 1,
            Y = 1,
            Properties = new List<PropertyInfo> { new() { Id = 30005, Name = "Name" } }
        });

        await service.UnsubscribeAsync(new ElementInfo { X = 1, Y = 1 });

        Assert.Equal(1, fabric.Observers[0].StopCount);
    }

    [Fact]
    public async Task UnsubscribeAsync_WithNullInfo_DoesNothing()
    {
        var element = new FakeElement();
        var provider = new FakeProvider { FindElementResult = element };
        var fabric = new FakeTargetFabric();
        var manager = new SurveillanceManager(provider, fabric);
        var service = new SurveillanceService(manager, new Convertor(provider), provider);

        await service.UnsubscribeAsync(null!);

        Assert.Equal(0, provider.FindElementCalls);
    }
}
