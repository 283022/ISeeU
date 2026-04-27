using System.Drawing;
using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class UIAutomationServiceWindows : IUAtomationProvider
{
    private readonly CUIAutomation8 _automation = new CUIAutomation8();
    public UIAutomationServiceWindows()
    {
        var desktop = _automation.GetRootElement();
    }
    public IElement FindElement(Point location)
    {
        var rect = new tagPOINT { x = location.X, y = location.Y };
        var element = _automation.ElementFromPoint(rect);
        return new WindowsElement(element);
    }

    public IElement GetFocusedElement()
    {
        var element = _automation.GetFocusedElement();
        return new WindowsElement(element);
    }

    public int[] GetSupportedProperties(IElement element)
    {
        throw new NotImplementedException();
    }

    /*
    public int[] GetSupportedProperties(IElement element)
    {
        if (element is WindowsElement elementWin)
        {
            elementWin.GetNativeElement();
        }
    }
*/
    
    public ITargetObserver CreateTargetObserver(int propertyId, Action<int, object> onPropertyChanged)
    {
        throw new NotImplementedException();
    }

    /*
    Not sure about this method. 'cause in abstract classes it doesn't need 
    
    public CUIAutomation8 GetCUIAutomation()
    {
        return _automation;
    }
    */
}