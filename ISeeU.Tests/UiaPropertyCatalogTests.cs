using ConnectInfo;

namespace ISeeU.Tests;

public class UiaPropertyCatalogTests
{
    [Fact]
    public void All_ContainsExpectedProperties()
    {
        Assert.Equal(5, UiaPropertyCatalog.All.Count);
        Assert.Contains(UiaPropertyCatalog.All, p => p.Id == 30005 && p.Name == "Name");
    }

    [Fact]
    public void TryGet_ReturnsTrue_ForKnownPatternProperty()
    {
        var found = UiaPropertyCatalog.TryGet(30045, out var prop);

        Assert.True(found);
        Assert.Equal("Value", prop.Name);
        Assert.Equal(ObserveStrategy.Polling, prop.Strategy);
        Assert.Equal(30043, prop.PatternAvailabilityId);
    }

    [Fact]
    public void TryGet_ReturnsFalse_ForUnknownId()
    {
        Assert.False(UiaPropertyCatalog.TryGet(99999, out _));
    }

    [Fact]
    public void Get_ReturnsProperty_OrNull()
    {
        var toggle = UiaPropertyCatalog.Get(30086);
        Assert.NotNull(toggle);
        Assert.Equal("Toggle State", toggle!.DisplayName);
        Assert.Equal(UiaValueKind.Toggle, toggle.Kind);
        Assert.Equal(30041, toggle.PatternAvailabilityId);

        Assert.Null(UiaPropertyCatalog.Get(99999));
    }

    [Fact]
    public void IsKnown_ReflectsCatalog()
    {
        Assert.True(UiaPropertyCatalog.IsKnown(30010));
        Assert.False(UiaPropertyCatalog.IsKnown(123));
    }

    [Fact]
    public void Resolve_DropsUnknownIds()
    {
        var resolved = UiaPropertyCatalog.Resolve(new[] { 30005, 99999, 30086 });

        Assert.Equal(2, resolved.Count);
        Assert.Contains(resolved, p => p.Id == 30005);
        Assert.Contains(resolved, p => p.Id == 30086);
        Assert.DoesNotContain(resolved, p => p.Id == 99999);
    }
}
