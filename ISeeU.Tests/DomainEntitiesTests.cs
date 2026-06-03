using ConnectInfo;
using ISeeU.Domain.Entities;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Tests;

public class DomainEntitiesTests
{
    [Fact]
    public void TargetInfo_CopiesElementData()
    {
        var element = new FakeElement { Name = "Edit", ProcessId = 7 };
        var before = DateTime.Now;

        var target = new TargetInfo(element);

        Assert.Equal("Edit", target.Name);
        Assert.Same(element, target.Element);
        Assert.NotNull(target.Changes);
        Assert.Empty(target.Changes);
        Assert.True(target.SubscribeTime >= before);
    }

    [Fact]
    public void SurveillanceRule_ExposesConstructorArguments()
    {
        var element = new FakeElement();
        var target = new TargetInfo(element);
        var observer = new FakeObserver();
        Action<int, object> callback = (_, _) => { };

        var rule = new SurveillanceRule(30005, target, callback, observer);

        Assert.Equal(30005, rule.PropertyId);
        Assert.Same(target, rule.Target);
        Assert.Same(callback, rule.Callback);
        Assert.Same(observer, rule.Observer);
    }

    [Fact]
    public void PropertyChange_StoresValues()
    {
        var now = DateTime.Now;
        var change = new PropertyChange { PropertyId = 30045, NewValue = "abc", Timestamp = now };

        Assert.Equal(30045, change.PropertyId);
        Assert.Equal("abc", change.NewValue);
        Assert.Equal(now, change.Timestamp);
    }

    [Fact]
    public void Rect_StoresGeometry()
    {
        var rect = new Rect { X = 1, Y = 2, Width = 3, Height = 4 };

        Assert.Equal(1, rect.X);
        Assert.Equal(2, rect.Y);
        Assert.Equal(3, rect.Width);
        Assert.Equal(4, rect.Height);
    }

    [Fact]
    public void ElementInfo_And_PropertyInfo_RoundTripValues()
    {
        var info = new ElementInfo
        {
            ElementId = "id-1",
            Name = "win",
            X = 10,
            Y = 20,
            Width = 30,
            Height = 40,
            Properties = new List<PropertyInfo> { new() { Id = 30005, Name = "Name" } }
        };

        Assert.Equal("id-1", info.ElementId);
        Assert.Equal("win", info.Name);
        Assert.Equal(10, info.X);
        Assert.Equal(20, info.Y);
        Assert.Equal(30, info.Width);
        Assert.Equal(40, info.Height);
        Assert.Single(info.Properties);
        Assert.Equal(30005, info.Properties[0].Id);
        Assert.Equal("Name", info.Properties[0].Name);
    }
}
