using ISeeU.Domain.Interfaces;

namespace ISeeU.Application.Contracts;

public interface IUIAutomationProvider
{
    public IElement FindElement(System.Drawing.Point location);
    public IElement GetFocusedElement();
    
    
    public Dictionary<int, string>  GetSupportedProperties(IElement element);

    // Низкоуровневое чтение значения свойства по id (нужно, например,
    public object GetCurrentPropertyValue(IElement element, int propertyId);

    public bool ElementIsAlive(IElement element);
}