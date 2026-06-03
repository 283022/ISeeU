using ConnectInfo;
using Wpf;

namespace ISeeU.Tests;

public class WpfModelsTests
{
    [Fact]
    public void PropertyChoice_KindLabel_PrefixesKind()
    {
        var choice = new PropertyChoice { Kind = "Bool" };

        Assert.Equal("· Bool", choice.KindLabel);
    }

    [Fact]
    public void MonitoredElement_Name_UsesElementInfoOrEmpty()
    {
        Assert.Equal("", new MonitoredElement { ElementInfo = null }.Name);
        Assert.Equal("Win", new MonitoredElement { ElementInfo = new ElementInfo { Name = "Win" } }.Name);
    }
}
