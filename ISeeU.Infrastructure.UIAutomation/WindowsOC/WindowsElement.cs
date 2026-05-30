using Interop.UIAutomationClient;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class WindowsElement : IElement
{
    private IUIAutomationElement _nativeElement;

    public string Name => _nativeElement.CurrentName;
    public int ProcessId => _nativeElement.CurrentProcessId;
    public int ControlType => _nativeElement.CurrentControlType;

    private CUIAutomation8 _automation8;
    
    public Rect BoundingRectangle { get; }

    public WindowsElement(IUIAutomationElement nativeElement, CUIAutomation8 automation8)
    {
        _nativeElement = nativeElement;
        _automation8 = automation8;
        // Заполняем BoundingRectangle из COM-объекта
        var rect = nativeElement.CurrentBoundingRectangle;
        BoundingRectangle = new Rect
        {
            X = rect.left,
            Y = rect.top,
            Width = rect.right - rect.left,
            Height = rect.bottom - rect.top
        };
    }
    


    public int[] GetSupportedProperties()
    {
        _automation8.PollForPotentialSupportedProperties(_nativeElement, out int[] propertyIds,
            out string[] supportedProperties);

        for (var i = 0; i < propertyIds.Length; i++)
        {
            Console.WriteLine($"{propertyIds[i]},  {supportedProperties[i]}");
        }

        return propertyIds;
    }

    public string[] GetSupportedPropertiesForHuman()
    {
        _automation8.PollForPotentialSupportedProperties(_nativeElement, out int[] propertyIds,
            out string[] supportedProperties);
        return supportedProperties;
    }


    public IUIAutomationElement GetNativeElement()
    {
        return _nativeElement;
    }
}