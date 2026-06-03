using ConnectInfo;
using Wpf;

namespace ISeeU.Tests;

public class SubscriptionBuilderTests
{
    [Fact]
    public void ToChoice_MapsCatalogProperty()
    {
        var prop = UiaPropertyCatalog.Get(30045)!; // Value

        var choice = SubscriptionBuilder.ToChoice(prop);

        Assert.Equal(30045, choice.Id);
        Assert.Equal("Value", choice.Name);
        Assert.Equal("Value", choice.DisplayName);
        Assert.Equal("String", choice.Kind);
        Assert.True(choice.IsSelected);
    }

    [Fact]
    public void SelectedProperties_KeepsOnlyChecked()
    {
        var choices = new[]
        {
            new PropertyChoice { Id = 1, Name = "A", IsSelected = true },
            new PropertyChoice { Id = 2, Name = "B", IsSelected = false },
            new PropertyChoice { Id = 3, Name = "C", IsSelected = true },
        };

        var selected = SubscriptionBuilder.SelectedProperties(choices);

        Assert.Equal(new[] { 1, 3 }, selected.Select(p => p.Id));
        Assert.Equal(new[] { "A", "C" }, selected.Select(p => p.Name));
    }

    [Fact]
    public void Build_CopiesCurrentElement_AndKeepsExistingId()
    {
        var current = new ElementInfo { ElementId = "id-7", Name = "W", X = 3, Y = 4, Width = 5, Height = 6 };
        var chosen = new List<PropertyInfo> { new() { Id = 30005, Name = "Name" } };

        var result = SubscriptionBuilder.Build(current, chosen);

        Assert.Equal("id-7", result.ElementId);
        Assert.Equal("W", result.Name);
        Assert.Equal(3, result.X);
        Assert.Equal(4, result.Y);
        Assert.Equal(5, result.Width);
        Assert.Equal(6, result.Height);
        Assert.Same(chosen, result.Properties);
    }

    [Fact]
    public void Build_GeneratesId_WhenMissing()
    {
        var current = new ElementInfo { ElementId = null!, Name = "W" };

        var result = SubscriptionBuilder.Build(current, new List<PropertyInfo>());

        Assert.True(Guid.TryParse(result.ElementId, out _));
    }
}
