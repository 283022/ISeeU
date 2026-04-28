using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class TargetFabricWinApi30(CUIAutomation8 automation): ITargetFabric
{
    public ITargetObserver CreateTargetObserver(int propertyId, Action<int, object> onPropertyChanged)
    {
        
        return new TargetObserverWinApi30(propertyId, onPropertyChanged,automation);
    }
}
