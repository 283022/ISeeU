using ConnectInfo;
using ISeeU.Infrastructure.UIAutomation.WindowsOC;

namespace ISeeU.Tests;

public class ObserveStrategyResolverTests
{
    public static IEnumerable<object[]> Cases => new[]
    {
        new object[] { 30005, ObserveStrategy.Event },   // Name
        new object[] { 30010, ObserveStrategy.Event },   // IsEnabled
        new object[] { 30022, ObserveStrategy.Event },   // IsOffscreen
        new object[] { 30045, ObserveStrategy.Polling }, // Value (за паттерном)
        new object[] { 30086, ObserveStrategy.Polling }, // ToggleState (за паттерном)
        new object[] { 99999, ObserveStrategy.Event },   // неизвестное -> Event по умолчанию
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Resolve_PicksStrategyFromCatalog(int propertyId, ObserveStrategy expected)
    {
        Assert.Equal(expected, ObserveStrategyResolver.Resolve(propertyId));
    }
}
