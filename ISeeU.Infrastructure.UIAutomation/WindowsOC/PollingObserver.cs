using Interop.UIAutomationClient;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class PollingObserver : ITargetObserver
{
    private readonly IUIAutomationElement _element;
    private readonly int _propertyId;
    private readonly Action<int, object> _callback;
    private Timer? _timer;
    private string? _lastValue;

    public PollingObserver(IUIAutomationElement element, int propertyId, Action<int, object> callback)
    {
        _element = element;
        _propertyId = propertyId;
        _callback = callback;
    }

    public void Start()
    {
        _lastValue = GetCurrentValue();
        _timer = new Timer(_ => Poll(), null, 300, 300);
    }

    private void Poll()
    {
        var newValue = GetCurrentValue();
        if (_lastValue != newValue)
        {
            _lastValue = newValue;
            _callback(_propertyId, newValue ?? "");
        }
    }

    private string? GetCurrentValue()
    {
        try
        {
            if (_element == null) return null;

            var pattern = (IUIAutomationValuePattern)_element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);
            
            return pattern.CurrentValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetValue] Error: {ex.Message}");
            return null;
        }
    }

    public void Stop() => _timer?.Dispose();
}