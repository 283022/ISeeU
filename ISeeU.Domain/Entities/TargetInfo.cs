namespace ISeeU.Domain.Entities;

public class TargetInfo(string name)
{
    public string Name { get; } = name;
    public DateTime SubscribeTime { get; } = DateTime.Now;
    public  List<PropertyChange> Changes { get; set; } = new();
    
}

public class PropertyChange
{
    public int PropertyId { get; set; }
    public object NewValue { get; set; }
    public DateTime Timestamp { get; set; } 
}