using Interop.UIAutomationClient;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class WindowsElement(IUIAutomationElement nativeElement, CUIAutomation8 automation8) : IElement
{
    private IUIAutomationElement _nativeElement = nativeElement;

    public string Name => _nativeElement.CurrentName;
    public int ProcessId => _nativeElement.CurrentProcessId;
    public int ControlType => _nativeElement.CurrentControlType;
    

    public string[] GetSupportedProperties()
    { 
       automation8.PollForPotentialSupportedProperties(_nativeElement, out int[] propertyIds,
            out string[] supportedProperties);
        return supportedProperties;
    }
    
    public IUIAutomationElement  GetNativeElement()
    {
        return _nativeElement;
    }
}