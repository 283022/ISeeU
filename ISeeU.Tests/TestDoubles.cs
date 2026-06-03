using ISeeU.Application.Contracts;
using ISeeU.Domain.Interfaces;

namespace ISeeU.Tests;

// Простые тестовые двойники для интерфейсов слоёв Application/Domain.
// Реальные реализации (UIA/COM, named pipes) сюда не тащим.

internal sealed class FakeElement : IElement
{
    public string Name { get; init; } = "element";
    public int ProcessId { get; init; } = 1;
    public int ControlType { get; init; } = 0;
    public Rect BoundingRectangle { get; init; }
    public int[] SupportedProps { get; init; } = Array.Empty<int>();
    public string[] SupportedPropsHuman { get; init; } = Array.Empty<string>();

    public int[] GetSupportedProperties() => SupportedProps;
    public string[] GetSupportedPropertiesForHuman() => SupportedPropsHuman;
}

internal sealed class FakeObserver : ITargetObserver
{
    public int StartCount { get; private set; }
    public int StopCount { get; private set; }

    public void Start() => StartCount++;
    public void Stop() => StopCount++;
}

internal sealed class FakeTargetFabric : ITargetFabric
{
    public List<FakeObserver> Observers { get; } = new();
    public List<(IElement Element, int PropertyId, Action<int, object> Callback)> Created { get; } = new();

    public ITargetObserver CreateTargetObserver(IElement element, int propertyId, Action<int, object> onPropertyChanged)
    {
        var observer = new FakeObserver();
        Observers.Add(observer);
        Created.Add((element, propertyId, onPropertyChanged));
        return observer;
    }
}

internal sealed class FakeProvider : IUIAutomationProvider
{
    public Func<Point, IElement>? FindElementFunc { get; set; }
    public IElement? FindElementResult { get; set; }
    public int FindElementCalls { get; private set; }

    public Dictionary<int, string> SupportedProperties { get; set; } = new();
    public Func<IElement, int, object>? CurrentValueFunc { get; set; }
    public Func<IElement, bool>? IsAliveFunc { get; set; }

    public IElement FindElement(Point location)
    {
        FindElementCalls++;
        if (FindElementFunc != null) return FindElementFunc(location);
        return FindElementResult!;
    }

    public IElement GetFocusedElement() => throw new NotSupportedException();

    public Dictionary<int, string> GetSupportedProperties(IElement element) => SupportedProperties;

    public object GetCurrentPropertyValue(IElement element, int propertyId) => CurrentValueFunc!(element, propertyId);

    public bool ElementIsAlive(IElement element) => IsAliveFunc?.Invoke(element) ?? true;
}

internal sealed class FakeClient : ISurveillanceClient
{
    public List<(string Element, string Property, string Value)> Calls { get; } = new();

    public void OnElementPropertyChanged(string elementName, string propertyName, string value)
        => Calls.Add((elementName, propertyName, value));
}
