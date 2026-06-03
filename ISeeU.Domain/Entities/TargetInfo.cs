using ISeeU.Domain.Interfaces;

namespace ISeeU.Domain.Entities;

public class TargetInfo(IElement element)
{
    public string Name { get; } = element.Name;
    public DateTime SubscribeTime { get; } = DateTime.Now;
    public List<PropertyChange> Changes { get; set; } = new();
    public IElement Element { get; } = element;
}

public class PropertyChange
{
    public int PropertyId { get; set; }
    public object? NewValue { get; set; }
    public DateTime Timestamp { get; set; }
}