using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class TargetFabricWinApi30(IUIAutomation2 automation2) : ITargetFabric
{
    private readonly HashSet<int> _pollingRequired = new() { 30045, 30041 }; // Value, ToggleState
    private readonly IUIAutomation2 _automation = automation2;
    
    public ITargetObserver CreateTargetObserver(IElement element, int propertyId, Action<int, object> onPropertyChanged)
    {
        var nativeElement = ((WindowsElement)element).GetNativeElement();
        
        if (_pollingRequired.Contains(propertyId))
            return new PollingObserver(nativeElement, propertyId, onPropertyChanged);
        
        return new TargetObserverWinApi30(element, propertyId, onPropertyChanged, _automation);
    }
}
