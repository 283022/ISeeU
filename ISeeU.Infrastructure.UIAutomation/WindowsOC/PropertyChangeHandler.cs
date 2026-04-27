using Interop.UIAutomationClient;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class PropertyChangeHandler(int id, Action<int, object> callback)
    : IUIAutomationPropertyChangedEventHandler
{
    public void HandlePropertyChangedEvent(IUIAutomationElement sender, int propertyId, object newValue)
    {
        if (propertyId == id)
            callback(propertyId, newValue);
    }
}