using System.Drawing;
using System.Runtime.InteropServices;
using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class UIAutomationServiceWindows : IUIAutomationProvider
{
    private readonly CUIAutomation8 _automation = new();
    
    public IElement FindElement(Point location)
    {
        var rect = new tagPOINT { x = location.X, y = location.Y };
        var element = _automation.ElementFromPoint(rect);
        return new WindowsElement(element, _automation);
    }

    public IElement GetFocusedElement()
    {
        var element = _automation.GetFocusedElement();
        return new WindowsElement(element, _automation);
    }
    
    public Dictionary<int, string> GetSupportedProperties(IElement element)
    {
        if (element is not IUIAutomationElement uiElement) 
            throw new NotSupportedException("Unsupported element type");
    
        _automation.PollForPotentialSupportedProperties(uiElement, out int[] propertyIds,
            out string[] supportedProperties);
    
        var result = new Dictionary<int, string>();
        for (var i = 0; i < propertyIds.Length; i++)
        {
            var cleanName = supportedProperties[i];
            result[propertyIds[i]] = cleanName;
        }
        return result;
    }

    public bool ElementIsAlive(IElement element)
    {
        //checking if the element is dead 
        if (element is not WindowsElement winElement) throw new  NotSupportedException("Unsupported element type");
        //try to give him nint; if element is dead - handle = 0, else - handle!=0
        var handle = winElement.GetNativeElement().CurrentNativeWindowHandle;
        return handle != IntPtr.Zero;
    }


    public CUIAutomation8 GetCUIAutomation()
    {
        return _automation;
    }
}