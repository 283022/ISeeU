namespace ConnectInfo;

public class ElementInfo
{
    public string? ElementId { get; set; }
    public string Name { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    // null = "свойства не заданы" (используется как маркер в SubscribeAsync).
    public List<PropertyInfo>? Properties { get; set; }
}



