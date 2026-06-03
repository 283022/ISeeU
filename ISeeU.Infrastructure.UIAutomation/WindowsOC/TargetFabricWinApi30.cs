using ConnectInfo;
using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class TargetFabricWinApi30(IUIAutomation2 automation2) : ITargetFabric
{
    private readonly IUIAutomation2 _automation = automation2;

    public ITargetObserver CreateTargetObserver(IElement element, int propertyId, Action<int, object> onPropertyChanged)
    {
        var nativeElement = ((WindowsElement)element).GetNativeElement();

        // Стратегия наблюдения берётся из единого каталога, а не хардкода.
        // Неизвестные свойства по умолчанию пробуем через UIA-событие.
        var strategy = UiaPropertyCatalog.Get(propertyId)?.Strategy ?? ObserveStrategy.Event;

        if (strategy == ObserveStrategy.Polling)
            return new PollingObserver(nativeElement, propertyId, onPropertyChanged);

        return new TargetObserverWinApi30(element, propertyId, onPropertyChanged, _automation);
    }
}
