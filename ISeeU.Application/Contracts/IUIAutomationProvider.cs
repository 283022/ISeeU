using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Contracts;

public interface IUIAutomationProvider
{
    public IElement FindElement(System.Drawing.Point location);
    public IElement GetFocusedElement();
    
    /* I'm not sure about these methods
    void SubscribeToFocusChange(IUIAutomationFocusChangedEventHandler handler);
    void UnsubscribeFromFocusChange(IUIAutomationFocusChangedEventHandler handler);
    */
    
    public string[] GetSupportedProperties(IElement element);

    public bool ElementIsAlive(IElement element);
}