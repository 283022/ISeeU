using ConnectInfo;
using ISeeU.Application.Convertor;

namespace ISeeU.Tests;

public class ConvertorTests
{
    [Fact]
    public void ToDomain_ResolvesElementByDtoCoordinates()
    {
        var element = new FakeElement { Name = "Target" };
        Point captured = default;
        var provider = new FakeProvider
        {
            FindElementFunc = p => { captured = p; return element; }
        };
        var convertor = new Convertor(provider);

        var result = convertor.ToDomain(new ElementInfo { X = 12, Y = 34 });

        Assert.Same(element, result);
        Assert.Equal(new Point(12, 34), captured);
    }
}
