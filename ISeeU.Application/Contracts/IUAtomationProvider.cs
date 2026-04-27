namespace ISeeU.Application.Contracts;

public interface IUAtomationProvider
{
    public IElement FindElement(System.Drawing.Point location);
    public IElement GetFocusedElement();
    
    /* I'm not sure about these methods
    void SubscribeToFocusChange(IUIAutomationFocusChangedEventHandler handler);
    void UnsubscribeFromFocusChange(IUIAutomationFocusChangedEventHandler handler);
    */
    
    int[] GetSupportedProperties(IElement element);
    ITargetObserver CreateTargetObserver(int propertyId, Action<int,object> onPropertyChanged);
    
}