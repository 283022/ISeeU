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
    
    public Dictionary<int, string>  GetSupportedProperties(IElement element);

    // Низкоуровневое чтение значения свойства по id (нужно, например,
    // чтобы проверить IsXxxPatternAvailable при обнаружении поддержки).
    public object GetCurrentPropertyValue(IElement element, int propertyId);

    public bool ElementIsAlive(IElement element);
}