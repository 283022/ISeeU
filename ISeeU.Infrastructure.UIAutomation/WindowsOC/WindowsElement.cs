using Interop.UIAutomationClient;
using ISeeU.Application.Contracts;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class WindowsElement(IUIAutomationElement nativeElement) : IElement
{
    private IUIAutomationElement _nativeElement = nativeElement;

    public string Name => _nativeElement.CurrentName;
    public int ProcessId => _nativeElement.CurrentProcessId;
    public int ControlType => _nativeElement.CurrentControlType;
    

    public int[] GetSupportedProperties()
    { 
        throw new NotImplementedException();
    }
    
    public IUIAutomationElement  GetNativeElement()
    {
        return _nativeElement;
    }
}