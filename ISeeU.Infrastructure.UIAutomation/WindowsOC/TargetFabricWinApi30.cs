using System.Diagnostics.CodeAnalysis;
using ConnectInfo;
using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

// Привязана к COM (каст к WindowsElement + конкретные наблюдатели),
// поэтому исключена из покрытия. Чистое правило выбора стратегии — в ObserveStrategyResolver.
[ExcludeFromCodeCoverage]
public class TargetFabricWinApi30(IUIAutomation2 automation2) : ITargetFabric
{
    private readonly IUIAutomation2 _automation = automation2;

    public ITargetObserver CreateTargetObserver(IElement element, int propertyId, Action<int, object> onPropertyChanged)
    {
        var nativeElement = ((WindowsElement)element).GetNativeElement();

        var strategy = ObserveStrategyResolver.Resolve(propertyId);

        if (strategy == ObserveStrategy.Polling)
            return new PollingObserver(nativeElement, propertyId, onPropertyChanged);

        return new TargetObserverWinApi30(element, propertyId, onPropertyChanged, _automation);
    }
}
