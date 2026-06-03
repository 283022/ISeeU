using System.Diagnostics.CodeAnalysis;
using Interop.UIAutomationClient;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Infrastructure.UIAutomation.WindowsOC;

public class PollingObserver : ITargetObserver
{
    private readonly int _propertyId;
    private readonly Action<int, object> _callback;
    private readonly Func<string?> _readValue;
    private readonly int _dueMs;
    private readonly int _periodMs;
    private Timer? _timer;
    private string? _lastValue;

    // Тестируемый конструктор: способ чтения значения вынесен в делегат,
    // поэтому диффинг-логику можно проверить без COM/UIA.
    public PollingObserver(Func<string?> readValue, int propertyId, Action<int, object> callback,
        int dueMs = 300, int periodMs = 300)
    {
        _readValue = readValue;
        _propertyId = propertyId;
        _callback = callback;
        _dueMs = dueMs;
        _periodMs = periodMs;
    }

    // Боевой конструктор: значение читаем из COM value-pattern. Не покрываем тестами.
    [ExcludeFromCodeCoverage]
    public PollingObserver(IUIAutomationElement element, int propertyId, Action<int, object> callback)
        : this(() => ReadFromValuePattern(element), propertyId, callback)
    {
    }

    public void Start()
    {
        // Запоминаем стартовое значение, чтобы первый же Poll не дал ложного срабатывания.
        _lastValue = _readValue();
        _timer = new Timer(_ => PollOnce(), null, _dueMs, _periodMs);
    }

    // Одна итерация опроса: callback дёргаем ТОЛЬКО при реальном изменении значения.
    internal void PollOnce()
    {
        var newValue = _readValue();
        if (_lastValue != newValue)
        {
            _lastValue = newValue;
            _callback(_propertyId, newValue ?? "");
        }
    }

    public void Stop() => _timer?.Dispose();

    [ExcludeFromCodeCoverage]
    private static string? ReadFromValuePattern(IUIAutomationElement element)
    {
        try
        {
            if (element == null) return null;

            var pattern = (IUIAutomationValuePattern)element.GetCurrentPattern(UIA_PatternIds.UIA_ValuePatternId);
            return pattern.CurrentValue;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetValue] Error: {ex.Message}");
            return null;
        }
    }
}
