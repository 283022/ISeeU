using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class TargetObserverWinApi30 : ITargetObserver
{
    private readonly int _propertyId;
    private readonly Action<int, object> _callback;
    private readonly IUIAutomation2 _automation;
    private IUIAutomationElement _element;
    private PropertyChangeHandler _comHandler; // отдельный класс для COM

    public TargetObserverWinApi30(int propertyId, Action<int, object> callback, IUIAutomation2 automation)
    {
        _propertyId = propertyId;
        _callback = callback;
        _automation = automation;
        _comHandler = new PropertyChangeHandler(propertyId, callback);
    }

    public void Start(IElement element)
    {
        var winElement = (WindowsElement)element;
        _element = winElement.GetNativeElement();
        
        // Подписываем обработчик
        _automation.AddPropertyChangedEventHandler(
            _element, TreeScope.TreeScope_Element, null,_comHandler, [_propertyId]);
    }
    

    public void Stop()
    {
        if (_comHandler != null && _element != null)
            _automation.RemovePropertyChangedEventHandler(_element, _comHandler);
    }
}